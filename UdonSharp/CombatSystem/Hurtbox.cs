using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Hurtbox : UdonSharpBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [UdonSynced] private float currentHealth;
    [SerializeField] private bool useHealth = true;

    [Header("Hit Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioSource hitAudio;

    [Header("Damage Labels")]
    [SerializeField] private DamageLabelPool damageLabelPool;

    [Header("Events")]
    public UdonBehaviour onHitEvent;
    public string onHitEventName = "OnHit";
    public UdonBehaviour onDeathEvent;
    public string onDeathEventName = "OnDeath";

    [UdonSynced] private int hitCounter;
    private int localHitCounter;

    private Vector3 lastHitPoint;
    public Vector3 GetLastHitPoint() { return lastHitPoint; }

    private bool isDead;

    void Start()
    {
        if (useHealth)
        {
            currentHealth = maxHealth;
            RequestSerialization();
        }
    }

    public void ApplyDamage(float amount, Vector3 point, Vector3 normal)
    {
        if (isDead) return;

        lastHitPoint = point;

        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        if (useHealth && amount > 0f)
        {
            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                isDead = true;
            }
        }

        hitCounter++;
        RequestSerialization();
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayHitEffects), point, normal, amount);

        FireLocalEvents();
    }

    public override void OnDeserialization()
    {
        if (hitCounter != localHitCounter)
        {
            localHitCounter = hitCounter;

            if (useHealth && currentHealth <= 0f && !isDead)
            {
                isDead = true;
                if (Utilities.IsValid(onDeathEvent))
                    onDeathEvent.SendCustomEvent(onDeathEventName);
            }
            else if (!isDead)
            {
                if (Utilities.IsValid(onHitEvent))
                    onHitEvent.SendCustomEvent(onHitEventName);
            }
        }

        if (useHealth && currentHealth > 0f && isDead)
        {
            isDead = false;
        }
    }

    private void FireLocalEvents()
    {
        if (isDead && Utilities.IsValid(onDeathEvent))
        {
            onDeathEvent.SendCustomEvent(onDeathEventName);
        }
        else if (Utilities.IsValid(onHitEvent))
        {
            onHitEvent.SendCustomEvent(onHitEventName);
        }
    }

    [NetworkCallable]
    public void PlayHitEffects(Vector3 point, Vector3 normal, float damageAmount)
    {
        if (Utilities.IsValid(hitParticles))
        {
            hitParticles.transform.position = point;
            if (normal != Vector3.zero)
                hitParticles.transform.rotation = Quaternion.LookRotation(normal);
            hitParticles.Play();
        }

        if (Utilities.IsValid(hitAudio))
        {
            hitAudio.transform.position = point;
            hitAudio.Play();
        }

        SpawnDamageLabel(damageAmount, point);
    }

    private void SpawnDamageLabel(float damage, Vector3 point)
    {
        if (!Utilities.IsValid(damageLabelPool)) return;
        damageLabelPool.Show(damage, point);
    }

    public void Respawn()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        currentHealth = maxHealth;
        isDead = false;
        RequestSerialization();
    }

    public float GetHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public bool IsDead() { return isDead; }
}
