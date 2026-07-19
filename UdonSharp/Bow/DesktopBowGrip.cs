
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DesktopBowGrip : UdonSharpBehaviour
{
    [SerializeField] private Bow bow;
    [SerializeField] private float minForce = 0f;
    [SerializeField] private float maxForce = 1f;
    [SerializeField] private float chargeRate = 1.0f;

    [UdonSynced] private bool charging;

    private float fireForce;

    void Start()
    {
        fireForce = minForce;
        charging = false;
    }

    public void StartCharging()
    {
        if (!Networking.IsOwner(gameObject)) return;
        charging = true;
        RequestSerialization();
    }

    public void StopCharging()
    {
        if (!Networking.IsOwner(gameObject)) return;
        charging = false;
        RequestSerialization();
    }

    void Update()
    {
        if (charging)
        {
            fireForce += chargeRate * Time.deltaTime;
            if (fireForce > maxForce) fireForce = maxForce;
            bow.SetPullDistance(fireForce);
        }
        else
        {
            fireForce = 0.0f;
            bow.SetPullDistance(fireForce);
        }
    }

}
