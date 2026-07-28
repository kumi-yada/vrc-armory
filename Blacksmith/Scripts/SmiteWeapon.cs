using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmiteWeapon : UdonSharpBehaviour
{
    [System.NonSerialized] public string recipeName;
    [System.NonSerialized] public float currentHeat;
    [System.NonSerialized] public bool isHeated;
    [UdonSynced] public bool isHeld;

    [SerializeField] private float heatRate = 100f;
    [SerializeField] private float coolRate = 3f;
    [SerializeField] private float optimalFormingHeat = 750f;
    [SerializeField] private float maxHeat = 1000f;
    private float defaultCoolRate;

    [Header("Materials")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material unfinishedMaterial;
    private Material[] originalSharedMaterials;

    [Header("Smiting")]
    [SerializeField] private SmitePoint[] smitePoints;
    private int currentSmiteIndex;

    public SmitePoint GetActiveSmitePoint()
    {
        if (smitePoints == null || smitePoints.Length == 0) return null;

        while (currentSmiteIndex < smitePoints.Length && smitePoints[currentSmiteIndex].IsFinished)
            currentSmiteIndex++;

        if (currentSmiteIndex >= smitePoints.Length) return null;

        return smitePoints[currentSmiteIndex];
    }

    [Header("Heat Glow")]
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private Color[] heatColorRamp = new Color[]
    {
        Color.black,
        new Color(1f, 0f, 0f),
        new Color(1f, 0.5f, 0f),
        new Color(1f, 0.9f, 0f),
        new Color(1f, 1f, 1f),
    };
    private MaterialPropertyBlock propBlock;
    private Color currentEmission;
    private bool glowDirty;

    [System.NonSerialized] public bool isCompleted;
    [System.NonSerialized] public float qualityScore;
    [System.NonSerialized] public float[] hitScores;
    private int hitCount;
    private int totalAttempts;
    private bool wasCompleted;

    bool isInEditor;

    void Start()
    {
        isInEditor = Networking.LocalPlayer == null;
        defaultCoolRate = coolRate;
        hitScores = new float[5];
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
    }

    void Update()
    {
        if (isInEditor)
            return;

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

        if (isHeated)
        {
            float effectiveRate = heatRate;
            currentHeat = Mathf.Min(currentHeat + effectiveRate * Time.deltaTime, maxHeat);
        }
        else if (currentHeat > 0f)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolRate * Time.deltaTime);
        }

        if (isCompleted && !wasCompleted && targetRenderer != null)
        {
            wasCompleted = true;
            if (originalSharedMaterials != null)
                targetRenderer.sharedMaterials = originalSharedMaterials;
        }

        UpdateHeatGlow();
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

    public bool IsHeld()
    {
        return isHeld;
    }

    public float GetHeat()
    {
        return currentHeat;
    }

    public void SetCoolRate(float rate)
    {
        coolRate = rate;
    }

    public void ResetCoolRate()
    {
        coolRate = defaultCoolRate;
    }

    public void OnGrabbed()
    {
        isHeld = true;
        RequestSerialization();
    }

    public void OnReleased()
    {
        isHeld = false;
        RequestSerialization();
    }

    public void RecordHit(float accuracy)
    {
        if (isCompleted) return;

        totalAttempts++;

        if (accuracy <= 0f) return;

        float optimalHeat = optimalFormingHeat;
        float heatDelta = Mathf.Abs(currentHeat - optimalHeat);
        float heatTolerance = optimalHeat * 0.25f;
        float heatFactor = 1f - Mathf.Clamp01(heatDelta / heatTolerance);

        float hitScore = accuracy * 0.6f + heatFactor * 0.4f;

        if (hitCount < hitScores.Length)
        {
            hitScores[hitCount] = hitScore;
            hitCount++;
        }
    }

    public void EvaluateQuality()
    {
        if (isCompleted || hitCount == 0)
        {
            qualityScore = 0f;
            isCompleted = true;
            return;
        }

        float total = 0f;
        for (int i = 0; i < hitCount; i++)
            total += hitScores[i];

        float avgScore = total / hitCount;

        float variance = 0f;
        for (int i = 0; i < hitCount; i++)
        {
            float diff = hitScores[i] - avgScore;
            variance += diff * diff;
        }
        variance /= hitCount;
        float consistencyFactor = 1f - Mathf.Clamp01(variance * 4f);

        qualityScore = avgScore * 0.7f + consistencyFactor * 0.3f;
        isCompleted = true;
    }

    public string GetQualityLabel()
    {
        if (!isCompleted) return "Unfinished";

        if (qualityScore >= 0.9f) return "Masterwork";
        if (qualityScore >= 0.75f) return "Excellent";
        if (qualityScore >= 0.6f) return "Good";
        if (qualityScore >= 0.4f) return "Fair";
        if (qualityScore >= 0.2f) return "Poor";
        return "Ruined";
    }

    public float GetQualityScore()
    {
        return qualityScore;
    }

    public int GetHitCount()
    {
        return hitCount;
    }

    public int GetTotalAttempts()
    {
        return totalAttempts;
    }
}
