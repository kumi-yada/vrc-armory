using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    public void OnTriggerEnter(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        weapon.isHeated = false;
        weapon.coolRate = coolRate;
    }

    public void OnTriggerExit(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        weapon.ResetCoolRate();
    }
}
