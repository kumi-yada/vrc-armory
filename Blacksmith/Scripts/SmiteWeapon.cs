using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmiteWeapon : UdonSharpBehaviour
{
    [UdonSynced] [System.NonSerialized] public float currentHeat;
    [UdonSynced] [System.NonSerialized] public bool isHeated;
    [UdonSynced] [System.NonSerialized] public bool isHeld;
    [UdonSynced] [System.NonSerialized] private Vector3 syncedPosition;
    [UdonSynced] [System.NonSerialized] private Quaternion syncedRotation;

    [SerializeField] public string recipeName;
    [System.NonSerialized] public int spawnItemIndex;
    [SerializeField] private float heatRate = 100f;
    [SerializeField] [UdonSynced] public float coolRate = 3f;
    [SerializeField] private float minOptimalHeat = 562.5f;
    [SerializeField] private float maxHeat = 1000f;
    [System.NonSerialized] public float defaultCoolRate;


    [SerializeField] private SmitePoint[] smitePoints;
    [UdonSynced] private int currentSmiteIndex;

    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private Color[] heatColorRamp = new Color[]
    {
        Color.black,
        new Color(1f, 0f, 0f),
        new Color(1f, 0.5f, 0f),
        new Color(1f, 0.9f, 0f),
        new Color(1f, 1f, 1f),
    };

    [Header("Visuals")]
    [SerializeField] private SkinnedMeshRenderer blendShapeRenderer;
    [SerializeField] private int blobShapeIndex;
    [System.NonSerialized] private MaterialPropertyBlock propBlock;
    [System.NonSerialized] private Color currentEmission;
    [System.NonSerialized] private bool glowDirty;

    [UdonSynced] [System.NonSerialized] public bool isCompleted;
    [UdonSynced] [System.NonSerialized] public int finishTimeMs;
    [UdonSynced] [System.NonSerialized] public bool isDisplayed;
    [UdonSynced] [System.NonSerialized] public float qualityScore;
    [System.NonSerialized] public InventorySlot activeSlot;
    [System.NonSerialized] public Forge forge;
    [System.NonSerialized] private Storage storage;
    [SerializeField] private float experiencePerCompletion = 10f;
    [SerializeField] public float baseSellPrice = 10f;
    [System.NonSerialized] public float sellPrice;

    [System.NonSerialized] public int hitCount;
    [System.NonSerialized] private float runningMean;
    [System.NonSerialized] private float runningM2;

    [System.NonSerialized] public int totalAttempts;
    [System.NonSerialized] private bool completionInvalidated;

    [SerializeField] private VRC_Pickup pickup;

    [System.NonSerialized] bool isInEditor;

    void Awake()
    {
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
    }

    public void ResetState()
    {
        if (activeSlot != null)
        {
            activeSlot.Stash();
        }

        ApplyCraftingVisuals();
        isDisplayed = false;
        activeSlot = null;
        RequestSerialization();
        Debug.Log("SmiteWeapon: ResetState: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    private void ApplyCraftingVisuals()
    {
        currentHeat = 0f;
        isHeated = false;
        isHeld = false;
        isCompleted = false;
        qualityScore = 0f;
        sellPrice = 0f;
        finishTimeMs = 0;
        hitCount = 0;
        totalAttempts = 0;
        runningMean = 0f;
        runningM2 = 0f;
        coolRate = defaultCoolRate;
        currentSmiteIndex = 0;
        currentEmission = Color.black;
        glowDirty = true;
        completionInvalidated = false;

        if (blendShapeRenderer != null)
            blendShapeRenderer.SetBlendShapeWeight(blobShapeIndex, 100f);

        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        if (pickup != null) pickup.pickupable = false;

        var firstPoint = GetActiveSmitePoint();
        if (firstPoint != null)
            firstPoint.SetActive(true);
    }

    void Start()
    {
        isInEditor = Networking.LocalPlayer == null;
        defaultCoolRate = coolRate;
        hitCount = 0;
        totalAttempts = 0;
        qualityScore = 0f;
        isCompleted = false;

        if (!isInEditor)
        {
            var playerObjects = Networking.GetPlayerObjects(Networking.LocalPlayer);
            for (int i = 0; i < playerObjects.Length; i++)
            {
                if (!Utilities.IsValid(playerObjects[i])) continue;
                storage = playerObjects[i].GetComponentInChildren<Storage>();
                if (Utilities.IsValid(storage)) break;
            }
        }

        propBlock = new MaterialPropertyBlock();
        currentEmission = Color.black;
        glowDirty = true;

        if (!isInEditor)
            RequestSerialization();

        if (pickup != null) pickup.pickupable = false;

        var firstPoint = GetActiveSmitePoint();
        if (firstPoint != null)
            firstPoint.SetActive(true);
    }


    public override void OnPreSerialization()
    {
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
    }

    public override void OnDeserialization()
    {
        if (!isHeld)
            transform.SetPositionAndRotation(syncedPosition, syncedRotation);

        if (isCompleted && !isDisplayed)
            HidePosition();

        UpdateHeatGlow();
        UpdateBlobShape();
    }

    private void HidePosition()
    {
        transform.position = new Vector3(0f, -1000f, 0f);
        transform.rotation = Quaternion.identity;
    }

    public void Show(InventorySlot slot)
    {
        if (!Networking.IsOwner(gameObject)) return;
        activeSlot = slot;
        isDisplayed = true;

        if (pickup != null) pickup.pickupable = true;

        ApplyFinishedVisuals();
        if (Utilities.IsValid(slot))
        {
            qualityScore = slot.quality;
            finishTimeMs = slot.finishTimeMs;
            sellPrice = baseSellPrice * (1f + qualityScore);
        }

        RequestSerialization();
        Debug.Log("SmiteWeapon: Show: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    private void ApplyFinishedVisuals()
    {
        isCompleted = true;
        isHeated = false;
        currentHeat = 0f;
        currentSmiteIndex = smitePoints != null ? smitePoints.Length : 0;

        if (smitePoints != null)
        {
            for (int i = 0; i < smitePoints.Length; i++)
            {
                if (smitePoints[i] != null)
                    smitePoints[i].SetActive(false);
            }
        }

        UpdateBlobShape();

        currentEmission = Color.black;
        glowDirty = true;
        UpdateHeatGlow();

        if (pickup != null) pickup.pickupable = true;
    }

    public void Hide()
    {
        if (!Networking.IsOwner(gameObject)) return;

        activeSlot = null;
        isDisplayed = false;

        if (pickup != null)
        {
            pickup.Drop();
            pickup.pickupable = false;
        }
        HidePosition();

        RequestSerialization();
        Debug.Log("SmiteWeapon: Hide: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    void Update()
    {
        if (isInEditor) return;

        UpdateHeldPosition();
        ProcessHeatAndCoolOff();
        UpdateHeatGlow();
        UpdateHeatSlider();
    }

    private void UpdateHeldPosition()
    {
        if (isHeld)
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            if (Utilities.IsValid(owner))
            {
                Transform attachPt = FindAttachPoint(owner);
                if (Utilities.IsValid(attachPt))
                    transform.SetPositionAndRotation(attachPt.position, attachPt.rotation);
            }
        }
    }

    private void ProcessHeatAndCoolOff()
    {
        if (isHeated)
        {
            float effectiveRate = heatRate;
            if (forge != null)
                effectiveRate *= forge.GetHeatMultiplier();
            currentHeat = Mathf.Min(currentHeat + effectiveRate * Time.deltaTime, maxHeat);
        }
        else if (currentHeat > 0f)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolRate * Time.deltaTime);
        }

        bool allPointsDone = smitePoints != null && smitePoints.Length > 0 && currentSmiteIndex >= smitePoints.Length;

        if (!isCompleted && !completionInvalidated && allPointsDone && !isHeated && currentHeat <= 0f)
        {
            EvaluateQuality();
        }

        if (isCompleted && currentHeat > 0f)
        {
            isCompleted = false;
            completionInvalidated = true;
            RequestSerialization();
        }
    }

    private void UpdateHeatGlow()
    {
        if (blendShapeRenderer == null || propBlock == null) return;

        float t = Mathf.Clamp01(currentHeat / maxHeat);
        if (isCompleted) t = 0f;

        Color targetColor = Color.black;
        if (t > 0.001f)
        {
            targetColor = SampleHeatRamp(t);
            targetColor *= glowIntensity;
        }

        if (!glowDirty && targetColor == currentEmission) return;

        currentEmission = targetColor;
        blendShapeRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", targetColor);
        blendShapeRenderer.SetPropertyBlock(propBlock);
        glowDirty = false;
    }

    private void UpdateHeatSlider()
    {
        if (forge == null) return;
        if (forge.heatSlider == null) return;

        float norm = currentHeat / maxHeat;
        forge.heatSlider.value = norm;

        float low = minOptimalHeat / maxHeat;

        bool leftToRight = forge.heatSlider.direction == Slider.Direction.LeftToRight;
        bool bottomToTop = forge.heatSlider.direction == Slider.Direction.BottomToTop;

        float sliderHeight = forge.heatSlider.GetComponent<RectTransform>().rect.height;

        if (forge.optimalRangeMarkerLow != null)
        {
            if (leftToRight)
            {
                forge.optimalRangeMarkerLow.anchorMin = new Vector2(low, 0f);
                forge.optimalRangeMarkerLow.anchorMax = new Vector2(low, 1f);
                forge.optimalRangeMarkerLow.anchoredPosition = Vector2.zero;
            }
            else if (bottomToTop)
            {
                forge.optimalRangeMarkerLow.anchorMin = new Vector2(0f, 0f);
                forge.optimalRangeMarkerLow.anchorMax = new Vector2(1f, 0f);
                forge.optimalRangeMarkerLow.anchoredPosition = new Vector2(0f, low * sliderHeight);
            }
        }
    }

    public void AdvanceSmiteIndex()
    {
        if (smitePoints == null || smitePoints.Length == 0) return;

        var current = GetActiveSmitePoint();
        if (current != null)
            current.SetActive(false);

        currentSmiteIndex++;
        if (currentSmiteIndex > smitePoints.Length)
            currentSmiteIndex = smitePoints.Length;

        var next = GetActiveSmitePoint();
        if (next != null)
            next.SetActive(true);

        UpdateBlobShape();
        RequestSerialization();
        Debug.Log("SmiteWeapon: AdvanceSmiteIndex: currentSmiteIndex = " + currentSmiteIndex);
    }

    private void UpdateBlobShape()
    {
        if (blendShapeRenderer == null) return;

        float blobVal = smitePoints != null && smitePoints.Length > 0
            ? 100f * (1f - (float)currentSmiteIndex / smitePoints.Length)
            : 100f;
        blendShapeRenderer.SetBlendShapeWeight(blobShapeIndex, blobVal);
    }

    private Color SampleHeatRamp(float t)
    {
        int count = heatColorRamp.Length;
        if (count == 0) return Color.black;
        if (count == 1) return heatColorRamp[0];

        float scaled = t * (count - 1);
        int lower = Mathf.FloorToInt(scaled);
        int upper = Mathf.Min(lower + 1, count - 1);
        float lerp = scaled - lower;

        return Color.Lerp(heatColorRamp[lower], heatColorRamp[upper], lerp);
    }

    private Transform FindAttachPoint(VRCPlayerApi player)
    {
        var objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            Tongs tongs = objects[i].GetComponentInChildren<Tongs>();
            if (Utilities.IsValid(tongs)) return tongs.attachPoint;
        }
        return null;
    }

    public float GetHeatNormalized()
    {
        return currentHeat / maxHeat;
    }

    public bool IsHeatOptimal()
    {
        return currentHeat >= minOptimalHeat;
    }

    public bool IsHeatTooCold()
    {
        return currentHeat < minOptimalHeat;
    }

    public SmitePoint GetActiveSmitePoint()
    {
        if (smitePoints == null || smitePoints.Length == 0) return null;
        if (currentSmiteIndex >= smitePoints.Length) return null;
        return smitePoints[currentSmiteIndex];
    }

    public void ResetCoolRate()
    {
        if (!Networking.IsOwner(gameObject)) return;
        coolRate = defaultCoolRate;
        RequestSerialization();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (other.gameObject.name == "HeatArea")
        {
            Debug.Log("SmiteWeapon: OnTriggerEnter: HeatArea entered, isHeated = true");
            isHeated = true;
            RequestSerialization();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (other.gameObject.name == "HeatArea")
        {
            Debug.Log("SmiteWeapon: OnTriggerExit: HeatArea exited, isHeated = false");
            isHeated = false;
            RequestSerialization();
        }
    }

    public void OnGrabbed()
    {
        isHeld = true;
        RequestSerialization();
        Debug.Log("SmiteWeapon: OnGrabbed: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    public void OnReleased()
    {
        isHeld = false;
        RequestSerialization();
        Debug.Log("SmiteWeapon: OnReleased: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    public void RecordHit(float accuracy)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (isCompleted) return;
        Debug.Log("SmiteWeapon: RecordHit: accuracy = " + accuracy + ", currentHeat = " + currentHeat + ", minOptimalHeat = " + minOptimalHeat);

        totalAttempts++;

        if (accuracy <= 0f) return;

        float heatFactor = Mathf.Clamp01(currentHeat / minOptimalHeat);

        float hitScore = accuracy * 0.6f + heatFactor * 0.4f;

        hitCount++;
        float delta = hitScore - runningMean;
        runningMean += delta / hitCount;
        float delta2 = hitScore - runningMean;
        runningM2 += delta * delta2;
    }

    private void TryAutoStore()
    {
        if (!Utilities.IsValid(storage))
        {
            Debug.Log("Cannot autostore without storage");
            return;
        }

        Debug.Log("Autostoring item");
        storage.AutoStoreItem(this);

        if (Utilities.IsValid(forge))
            forge.ClearCurrentItem();
    }

    public void EvaluateQuality()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (isCompleted || hitCount == 0)
        {
            qualityScore = 0f;
            sellPrice = baseSellPrice;
            isCompleted = true;
            finishTimeMs = Networking.GetServerTimeInMilliseconds();
            RequestSerialization();
            AwardExperience();
            if (pickup != null) pickup.pickupable = true;
            TryAutoStore();
            return;
        }

        float avgScore = runningMean;
        float variance = hitCount > 1 ? runningM2 / hitCount : 0f;
        float consistencyFactor = 1f - Mathf.Clamp01(variance * 4f);

        qualityScore = avgScore * 0.7f + consistencyFactor * 0.3f;
        sellPrice = baseSellPrice * (1f + qualityScore);
        isCompleted = true;
        finishTimeMs = Networking.GetServerTimeInMilliseconds();
        RequestSerialization();
        AwardExperience();
        if (pickup != null) pickup.pickupable = true;
        TryAutoStore();
        Debug.Log("SmiteWeapon: EvaluateQuality: qualityScore = " + qualityScore + ", hitCount = " + hitCount + ", totalAttempts = " + totalAttempts);
    }

    private void AwardExperience()
    {
        float earned = experiencePerCompletion * qualityScore;
        if (earned <= 0f) return;

        float currentExp = PlayerData.GetFloat(Networking.LocalPlayer, BlacksmithData.EXP_KEY);
        PlayerData.SetFloat(BlacksmithData.EXP_KEY, currentExp + earned);
        Debug.Log("SmiteWeapon: AwardExperience: earned = " + earned + ", new total = " + (currentExp + earned));
    }
}
