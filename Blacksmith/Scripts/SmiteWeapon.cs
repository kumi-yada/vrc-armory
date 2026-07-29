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
    [SerializeField] public float coolRate = 3f;
    [SerializeField] private float optimalFormingHeat = 750f;
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
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material unfinishedMaterial;
    [System.NonSerialized] private Material[] originalSharedMaterials;
    [System.NonSerialized] private MaterialPropertyBlock propBlock;
    [System.NonSerialized] private Color currentEmission;
    [System.NonSerialized] private bool glowDirty;

    [UdonSynced] [System.NonSerialized] public bool isCompleted;
    [System.NonSerialized] public bool isStored;
    [System.NonSerialized] public Forge forge;
    [UdonSynced] [System.NonSerialized] public float qualityScore;
    [SerializeField] private Storage storage;
    [SerializeField] private float experiencePerCompletion = 10f;

    [System.NonSerialized] public int hitCount;
    [System.NonSerialized] private float runningMean;
    [System.NonSerialized] private float runningM2;

    [System.NonSerialized] public int totalAttempts;
    [System.NonSerialized] private bool wasCompleted;

    [System.NonSerialized] bool isInEditor;

    void Awake()
    {
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
    }

    public void ResetState()
    {
        currentHeat = 0f;
        isHeated = false;
        isHeld = false;
        isCompleted = false;
        wasCompleted = false;
        qualityScore = 0f;
        hitCount = 0;
        totalAttempts = 0;
        runningMean = 0f;
        runningM2 = 0f;
        coolRate = defaultCoolRate;
        currentSmiteIndex = 0;
        currentEmission = Color.black;
        glowDirty = true;

        if (blendShapeRenderer != null)
            blendShapeRenderer.SetBlendShapeWeight(blobShapeIndex, 100f);

        if (targetRenderer != null)
        {
            if (unfinishedMaterial != null)
                targetRenderer.sharedMaterial = unfinishedMaterial;
        }

        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        var firstPoint = GetActiveSmitePoint();
        if (firstPoint != null)
            firstPoint.SetActive(true);

        RequestSerialization();
        Debug.Log("SmiteWeapon: ResetState: recipeName = " + recipeName + ", spawnItemIndex = " + spawnItemIndex);
    }

    void Start()
    {
        isInEditor = Networking.LocalPlayer == null;
        defaultCoolRate = coolRate;
        hitCount = 0;
        totalAttempts = 0;
        qualityScore = 0f;
        isCompleted = false;

        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            originalSharedMaterials = targetRenderer.sharedMaterials;
            if (unfinishedMaterial != null)
                targetRenderer.sharedMaterial = unfinishedMaterial;
        }

        propBlock = new MaterialPropertyBlock();
        currentEmission = Color.black;
        glowDirty = true;

        if (!isInEditor)
            RequestSerialization();

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

        if (!Networking.IsOwner(gameObject))
        {
            UpdateHeatGlow();
            UpdateBlobShape();
        }

        UpdateMaterialForCompleted();
    }

    private void UpdateMaterialForCompleted()
    {
        if (isCompleted && !wasCompleted && targetRenderer != null)
        {
            wasCompleted = true;
            if (originalSharedMaterials != null)
                targetRenderer.sharedMaterials = originalSharedMaterials;
        }
    }

    void Update()
    {
        if (isInEditor) return;

        UpdateHeldPosition();
        ProcessHeatAndCoolOff();
        UpdateMaterialForCompleted();
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
            currentHeat = Mathf.Min(currentHeat + effectiveRate * Time.deltaTime, maxHeat);
        }
        else if (currentHeat > 0f)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolRate * Time.deltaTime);
        }

        if (!isHeated && currentHeat <= 0f && hitCount > 0 && !isCompleted && coolRate > defaultCoolRate)
        {
            EvaluateQuality();
            ResetCoolRate();
        }
    }

    private void UpdateHeatGlow()
    {
        if (targetRenderer == null || propBlock == null) return;

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
        targetRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", targetColor);
        targetRenderer.SetPropertyBlock(propBlock);
        glowDirty = false;
    }

    private void UpdateHeatSlider()
    {
        if (forge == null) return;
        if (forge.heatSlider == null) return;

        float norm = currentHeat / maxHeat;
        forge.heatSlider.value = norm;

        float optNorm = optimalFormingHeat / maxHeat;
        float tol = (optimalFormingHeat * 0.25f) / maxHeat;
        float low = optNorm - tol;
        float high = optNorm + tol;

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

        if (forge.optimalRangeMarkerHigh != null)
        {
            if (leftToRight)
            {
                forge.optimalRangeMarkerHigh.anchorMin = new Vector2(high, 0f);
                forge.optimalRangeMarkerHigh.anchorMax = new Vector2(high, 1f);
                forge.optimalRangeMarkerHigh.anchoredPosition = Vector2.zero;
            }
            else if (bottomToTop)
            {
                forge.optimalRangeMarkerHigh.anchorMin = new Vector2(0f, 0f);
                forge.optimalRangeMarkerHigh.anchorMax = new Vector2(1f, 0f);
                forge.optimalRangeMarkerHigh.anchoredPosition = new Vector2(0f, high * sliderHeight);
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

    public SmitePoint GetActiveSmitePoint()
    {
        if (smitePoints == null || smitePoints.Length == 0) return null;
        if (currentSmiteIndex >= smitePoints.Length) return null;
        return smitePoints[currentSmiteIndex];
    }

    public void ResetCoolRate()
    {
        coolRate = defaultCoolRate;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

        Debug.Log("SmiteWeapon: OnTriggerEnter: other = " + other.gameObject.name);
        if (other.gameObject.name == "HeatArea")
        {
            isHeated = true;
            RequestSerialization();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

        Debug.Log("SmiteWeapon: OnTriggerExit: other = " + other.gameObject.name);
        if (other.gameObject.name == "HeatArea")
        {
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
        Debug.Log("SmiteWeapon: RecordHit: accuracy = " + accuracy + ", currentHeat = " + currentHeat + ", optimalFormingHeat = " + optimalFormingHeat);

        totalAttempts++;

        if (accuracy <= 0f) return;

        float optimalHeat = optimalFormingHeat;
        float heatDelta = Mathf.Abs(currentHeat - optimalHeat);
        float heatTolerance = optimalHeat * 0.25f;
        float heatFactor = 1f - Mathf.Clamp01(heatDelta / heatTolerance);

        float hitScore = accuracy * 0.6f + heatFactor * 0.4f;

        hitCount++;
        float delta = hitScore - runningMean;
        runningMean += delta / hitCount;
        float delta2 = hitScore - runningMean;
        runningM2 += delta * delta2;
    }

    private void TryAutoStore()
    {
        if (!Utilities.IsValid(storage)) return;
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
            isCompleted = true;
            RequestSerialization();
            AwardExperience();
            TryAutoStore();
            return;
        }

        float avgScore = runningMean;
        float variance = hitCount > 1 ? runningM2 / hitCount : 0f;
        float consistencyFactor = 1f - Mathf.Clamp01(variance * 4f);

        qualityScore = avgScore * 0.7f + consistencyFactor * 0.3f;
        isCompleted = true;
        RequestSerialization();
        AwardExperience();
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
