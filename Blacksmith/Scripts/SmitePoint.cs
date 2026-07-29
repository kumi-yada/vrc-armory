using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmitePoint : UdonSharpBehaviour
{
    [SerializeField] private float smiteSpeed = 30f;
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

    [Header("Effects")]
    [SerializeField] private ParticleSystem perfectParticles;
    [SerializeField] private AudioSource perfectAudio;
    [SerializeField] private ParticleSystem goodParticles;
    [SerializeField] private AudioSource goodAudio;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioSource hitAudio;

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

    public bool CheckHit()
    {
        if (!IsActive || IsFinished)
            return false;

        bool hit = SmiteValue >= hitAreaFrom && SmiteValue <= hitAreaTo;

        if (hit)
        {
            float center = (hitAreaFrom + hitAreaTo) / 2f;
            float halfRange = (hitAreaTo - hitAreaFrom) / 2f;
            float accuracy = 1f - Mathf.Abs(SmiteValue - center) / halfRange;

            if (Utilities.IsValid(weapon))
                weapon.RecordHit(accuracy);

            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlaySmiteEffects), accuracy);

            CurrentSmiteHits++;
            RandomizeHitArea();
            if (CurrentSmiteHits >= maxSmiteHits)
            {
                IsFinished = true;
                if (Utilities.IsValid(weapon))
                    weapon.AdvanceSmiteIndex();
            }
        }

        if (Utilities.IsValid(anvil))
            anvil.OnSmiteResult(hit);

        return hit;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        if (active && Utilities.IsValid(anvil))
        {
            anvil.SetActiveSmitePoint(this);
            anvil.ShowUI();
        }
        else if (!active && Utilities.IsValid(anvil))
        {
            anvil.SetActiveSmitePoint(null);
            anvil.HideUI();
        }
    }

    [NetworkCallable]
    public void PlaySmiteEffects(float accuracy)
    {
        if (accuracy >= 0.9f)
        {
            if (Utilities.IsValid(perfectParticles)) perfectParticles.Play();
            if (Utilities.IsValid(perfectAudio)) perfectAudio.Play();
        }
        else if (accuracy >= 0.6f)
        {
            if (Utilities.IsValid(goodParticles)) goodParticles.Play();
            if (Utilities.IsValid(goodAudio)) goodAudio.Play();
        }
        else
        {
            if (Utilities.IsValid(hitParticles)) hitParticles.Play();
            if (Utilities.IsValid(hitAudio)) hitAudio.Play();
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
