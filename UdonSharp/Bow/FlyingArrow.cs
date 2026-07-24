using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FlyingArrow : UdonSharpBehaviour
{
    [SerializeField] private float returnDelay = 5f;
    [SerializeField] private WeaponAutoScale weaponScale;

    private Rigidbody rb;

    // [UdonSynced] private Vector3 spawnPosition;
    // [UdonSynced] private Vector3 direction;
    // [UdonSynced] private float startSpeed;
    private bool hasHit = false;

    private VRCObjectPool pool;
    // private bool isInitialized = false;
    // private GameObject hitObject;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FindScaleComponent();
    }

    // public override void OnDeserialization()
    // {
    //     Initialize();
    //     if (hasHit)
    //     {
    //         rb.isKinematic = true;
    //         // if (hitObject != null)
    //         // {
    //         //     transform.SetParent(hitObject.transform);
    //         // }
    //     }
    // }

    // private void Initialize()
    // {
    //     if (isInitialized) return;
    //     if (rb == null) rb = GetComponent<Rigidbody>();

    //     isInitialized = true;
    //     transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(direction));
    //     rb.velocity = direction * startSpeed;
    // }

    private void LateUpdate()
    {
        if (!hasHit && rb != null && rb.velocity.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnHit(collision.collider);
        // hitObject = collision.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHit(other);
        // hitObject = other.gameObject;
    }

    private void OnHit(Collider other)
    {
        if (hasHit) return;
        hasHit = true;
        rb.isKinematic = true;

        if (!Networking.IsOwner(gameObject)) return;
        Debug.Log("Hit: " + other.name);
        // SendCustomEventDelayedSeconds(nameof(ReturnToPool), returnDelay);
        ReturnToPool();
    }

    public void ApplyForce(Vector3 spawnPosition, Vector3 direction, float speed, float avatarScale, VRCObjectPool pool)
    {
        if (!Networking.IsOwner(gameObject)) return;

        this.pool = pool;
        this.force = direction * speed;
        this.spawn = spawnPosition;
        this.avatarScale = avatarScale;
        SendCustomEventDelayedFrames(nameof(FireDelayed), 1);
    }

    private Vector3 force;
    private Vector3 spawn;
    private float avatarScale = 1f;

    private void FindScaleComponent()
    {
        if (!Utilities.IsValid(weaponScale)) weaponScale = GetComponent<WeaponAutoScale>();
    }

    public void FireDelayed()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Fire), spawn, force, avatarScale);
    }

    [NetworkCallable]
    public void Fire(Vector3 spawnPosition, Vector3 force, float avatarScale)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        hasHit = false;
        rb.isKinematic = false;
        FindScaleComponent();
        if (Utilities.IsValid(weaponScale)) weaponScale.ApplyScale(avatarScale);
        transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(force));
        rb.velocity = force;
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Return(gameObject);
        }
    }
}
