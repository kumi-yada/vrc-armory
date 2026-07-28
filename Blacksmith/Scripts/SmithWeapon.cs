using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmithWeapon : UdonSharpBehaviour
{
    [System.NonSerialized] public string recipeName;
    [System.NonSerialized] public float currentHeat;
    [System.NonSerialized] public bool isHeated;
    [UdonSynced] public bool isHeld;

    [SerializeField] private float heatRate = 100f;
    [SerializeField] private float coolRate = 3f;
    [SerializeField] private float optimalFormingHeat = 750f;
    private float defaultCoolRate;

    [System.NonSerialized] public bool isCompleted;
    [System.NonSerialized] public float qualityScore;
    [System.NonSerialized] public float[] hitScores;
    private int hitCount;
    private int totalAttempts;

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
            currentHeat = Mathf.Min(currentHeat + effectiveRate * Time.deltaTime, optimalFormingHeat);
        }
        else if (currentHeat > 0f)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolRate * Time.deltaTime);
        }
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
