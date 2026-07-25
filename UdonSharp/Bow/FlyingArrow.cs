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
    private Rigidbody rb;

    private bool hasHit = false;
    private bool stuck = false;
    public bool IsStuck => stuck;
    private Transform stuckTarget;
    private Vector3 stuckLocalPos;
    private Quaternion stuckLocalRot;

    private VRCObjectPool pool;

    private int shotOrder = int.MaxValue;
    public int GetShotOrder() => shotOrder;
    public void SetShotOrder(int order) => shotOrder = order;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (stuck)
        {
            if (stuckTarget != null)
            {
                transform.SetPositionAndRotation(
                    stuckTarget.TransformPoint(stuckLocalPos),
                    stuckTarget.rotation * stuckLocalRot
                );
            }
            return;
        }

        if (!hasHit && rb != null && rb.velocity.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPoint = collision.contacts[0].point;
        OnHit(collision.collider, hitPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHit(other, transform.position);
    }

    private void OnHit(Collider other, Vector3 hitPoint)
    {
        if (hasHit) return;
        hasHit = true;

        Stick(other, hitPoint);

        if (!Networking.IsOwner(gameObject)) return;
        Debug.Log("Hit: " + other.name);
    }

    private void Stick(Collider other, Vector3 hitPoint)
    {
        stuck = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        stuckTarget = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;

        stuckLocalPos = stuckTarget.InverseTransformPoint(hitPoint);
        stuckLocalRot = Quaternion.Inverse(stuckTarget.rotation) * transform.rotation;
    }

    public void ApplyForce(Vector3 spawnPosition, Vector3 direction, float speed, VRCObjectPool pool)
    {
        if (!Networking.IsOwner(gameObject)) return;

        this.pool = pool;
        this.force = direction * speed;
        this.spawn = spawnPosition;
        SendCustomEventDelayedFrames(nameof(FireDelayed), 1);
    }

    private Vector3 force;
    private Vector3 spawn;

    public void FireDelayed()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Fire), spawn, force);
    }

    [NetworkCallable]
    public void Fire(Vector3 spawnPosition, Vector3 force)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        hasHit = false;
        stuck = false;
        stuckTarget = null;
        shotOrder = int.MaxValue;
        rb.isKinematic = false;
        transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(force));
        rb.velocity = force;
    }

    public void ReturnToPool()
    {
        stuck = false;
        stuckTarget = null;
        shotOrder = int.MaxValue;
        if (pool != null)
        {
            pool.Return(gameObject);
        }
    }
}
