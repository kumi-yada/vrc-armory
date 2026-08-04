using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmitePoint : UdonSharpBehaviour
{
    [SerializeField] private float smiteSpeed = 100f;
    [SerializeField] private int maxSmiteHits = 5;
    [SerializeField] private float hitRange = 20f;
    [System.NonSerialized] public float hitAreaFrom;
    [System.NonSerialized] public float hitAreaTo;

    [System.NonSerialized] public float SmiteValue;
    [System.NonSerialized] public bool IsActive;
    [System.NonSerialized] public bool IsFinished;
    [System.NonSerialized] public int CurrentSmiteHits;
    [System.NonSerialized] public SmiteWeapon weapon;
    private Anvil anvil;

    private bool movingUp = true;

    void Start()
    {
        weapon = GetComponentInParent<SmiteWeapon>();
        RandomizeHitArea();
    }

    private void RandomizeHitArea()
    {
        float halfRange = hitRange / 2f;
        float center = Random.Range(halfRange, 100f - halfRange);
        hitAreaFrom = center - halfRange;
        hitAreaTo = center + halfRange;
    }

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

    public bool CanHit()
    {
        return IsActive && Utilities.IsValid(anvil) && !IsFinished;
    }

    public bool CheckHit()
    {
        if (!IsActive || IsFinished)
            return false;

        if (Utilities.IsValid(weapon))
            return false;

        bool optimalHeat = weapon.IsHeatOptimal();
        bool hit = SmiteValue >= hitAreaFrom && SmiteValue <= hitAreaTo && optimalHeat;

        if (hit)
        {
            float center = (hitAreaFrom + hitAreaTo) / 2f;
            float halfRange = (hitAreaTo - hitAreaFrom) / 2f;
            float accuracy = 1f - Mathf.Abs(SmiteValue - center) / halfRange;

            if (Utilities.IsValid(weapon))
                weapon.RecordHit(accuracy);

            if (Utilities.IsValid(anvil)) {
                anvil.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Anvil.PlaySmiteEffects), accuracy);
            }

            CurrentSmiteHits++;
            RandomizeHitArea();
            if (CurrentSmiteHits >= maxSmiteHits)
            {
                IsFinished = true;
                if (Utilities.IsValid(weapon))
                    weapon.AdvanceSmiteIndex();
            }
        }
        else
        {
            Missed();
        }

        if (Utilities.IsValid(anvil))
            anvil.OnSmiteResult(hit);

        return hit;
    }

    private void Missed() {
        if (Utilities.IsValid(weapon))
            weapon.RecordHit(0f);
        if (Utilities.IsValid(anvil)) {
            anvil.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Anvil.PlaySmiteEffects), 0f);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        if (active)
        {
            if (!Utilities.IsValid(anvil))
                DetectOverlappingAnvil();
            if (Utilities.IsValid(anvil))
            {
                anvil.SetActiveSmitePoint(this);
                anvil.ShowUI();
            }
        }
        else if (Utilities.IsValid(anvil))
        {
            anvil.SetActiveSmitePoint(null);
            anvil.HideUI();
            anvil = null;
        }
    }

    private void DetectOverlappingAnvil()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null) return;

        Collider[] hits = Physics.OverlapBox(
            collider.bounds.center,
            collider.bounds.extents,
            collider.transform.rotation,
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].name.Contains("Anvil")) continue;
            var hitAnvil = hits[i].GetComponent<Anvil>();
            if (Utilities.IsValid(hitAnvil))
            {
                anvil = hitAnvil;
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("Anvil")) return;
        if (IsFinished || !IsActive) return;

        var hitAnvil = other.GetComponent<Anvil>();
        if (!Utilities.IsValid(hitAnvil)) return;

        anvil = hitAnvil;
        anvil.SetActiveSmitePoint(this);
        anvil.ShowUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains("Anvil")) return;
        if (!Utilities.IsValid(anvil)) return;

        anvil.HideUI();
        anvil.SetActiveSmitePoint(null);
        anvil = null;
    }
}
