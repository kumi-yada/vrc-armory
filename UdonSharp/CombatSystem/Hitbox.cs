using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Hitbox : UdonSharpBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;

    [Header("Settings")]
    [SerializeField] private float hitCooldown = 0.5f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private bool active = true;

    private float lastHitTime;

    void Start()
    {
    }

    public void SetActive(bool state)
    {
        active = state;
        if (state)
            lastHitTime = 0f;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (!Networking.IsOwner(gameObject)) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var hurtBox = other.GetComponent<Hurtbox>();
        if (!Utilities.IsValid(hurtBox)) return;

        lastHitTime = Time.time;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;

        hurtBox.ApplyDamage(damage, hitPoint, hitNormal);
        Debug.Log($"Hitbox: Hit registered on {other.gameObject.name}. Damage: {damage}, HitPoint: {hitPoint}, HitNormal: {hitNormal}");
    }
}
