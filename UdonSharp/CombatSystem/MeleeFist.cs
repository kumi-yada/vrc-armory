
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MeleeFist : UdonSharpBehaviour
{
    [Header("References")]
    [SerializeField] private Hitbox hitbox;

    [Header("Punch Speed")]
    [SerializeField] private float minPunchSpeed = 3f;
    [SerializeField] private float maxDamageSpeed = 15f;
    [SerializeField] private float smoothing = 0.3f;

    [Header("Damage")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float maxDamage = 50f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private bool wasActive;
    private bool initialized;

    void Start()
    {
        if (!Utilities.IsValid(hitbox))
            hitbox = GetComponent<Hitbox>();

        lastPosition = transform.position;
        if (Utilities.IsValid(hitbox))
            hitbox.SetActive(false);
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;
        if (!Utilities.IsValid(hitbox)) return;

        Vector3 currentPos = transform.position;
        float rawSpeed = Vector3.Distance(currentPos, lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        float smoothFactor = Mathf.Clamp01(Time.deltaTime / Mathf.Max(smoothing, 0.0001f));
        currentSpeed = Mathf.Lerp(currentSpeed, rawSpeed, smoothFactor);

        bool shouldBeActive = currentSpeed >= minPunchSpeed;

        if (shouldBeActive && !wasActive)
        {
            float t = Mathf.InverseLerp(minPunchSpeed, maxDamageSpeed, currentSpeed);
            float damage = Mathf.Lerp(baseDamage, maxDamage, Mathf.Clamp01(t));
            hitbox.SetDamage(damage);
            hitbox.SetActive(true);
        }
        else if (!shouldBeActive && wasActive)
        {
            hitbox.SetActive(false);
        }
        else if (shouldBeActive)
        {
            float t = Mathf.InverseLerp(minPunchSpeed, maxDamageSpeed, currentSpeed);
            float damage = Mathf.Lerp(baseDamage, maxDamage, Mathf.Clamp01(t));
            hitbox.SetDamage(damage);
        }

        wasActive = shouldBeActive;
        lastPosition = currentPos;
    }
}
