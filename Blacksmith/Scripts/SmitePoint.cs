using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SmitePoint : UdonSharpBehaviour
{
    [SerializeField] private Anvil anvil;
    [SerializeField] private float smiteSpeed = 30f;
    [SerializeField] private int maxSmiteHits = 5;
    [SerializeField] private float hitAreaFrom = 40f;
    [SerializeField] private float hitAreaTo = 60f;

    public float SmiteValue { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFinished { get; private set; }
    public int CurrentSmiteHits { get; private set; }
    public float LastHitAccuracy { get; private set; }

    private bool movingUp = true;

    void Update()
    {
        if (!IsActive || IsFinished) return;

        if (movingUp)
        {
            SmiteValue += smiteSpeed * Time.deltaTime;
            if (SmiteValue >= 100f)
            {
                SmiteValue = 100f;
                movingUp = false;
            }
        }
        else
        {
            SmiteValue -= smiteSpeed * Time.deltaTime;
            if (SmiteValue <= 0f)
            {
                SmiteValue = 0f;
                movingUp = true;
            }
        }
    }

    public bool CheckHit()
    {
        if (!IsActive || IsFinished)
        {
            LastHitAccuracy = 0f;
            return false;
        }

        bool hit = SmiteValue >= hitAreaFrom && SmiteValue <= hitAreaTo;

        if (hit)
        {
            float center = (hitAreaFrom + hitAreaTo) / 2f;
            float halfRange = (hitAreaTo - hitAreaFrom) / 2f;
            LastHitAccuracy = 1f - Mathf.Abs(SmiteValue - center) / halfRange;

            CurrentSmiteHits++;
            if (CurrentSmiteHits >= maxSmiteHits)
            {
                IsFinished = true;
                IsActive = false;
                if (Utilities.IsValid(anvil))
                    anvil.HideUI();
            }
        }
        else
        {
            LastHitAccuracy = 0f;
        }

        return hit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Utilities.IsValid(anvil) || IsFinished) return;

        IsActive = true;
        anvil.ActiveSmitePoint = this;
        anvil.ShowUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Utilities.IsValid(anvil)) return;

        IsActive = false;
        if (anvil.ActiveSmitePoint == this)
            anvil.ActiveSmitePoint = null;
        anvil.HideUI();
    }
}
