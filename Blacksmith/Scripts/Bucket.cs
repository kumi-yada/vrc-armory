using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    private SmiteWeapon containedSmiteWeapon;

    public void OnTriggerEnter(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        containedSmiteWeapon = weapon;
        containedSmiteWeapon.isHeated = false;
        containedSmiteWeapon.coolRate = coolRate;
    }

    public void OnTriggerExit(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        if (containedSmiteWeapon == weapon)
        {
            containedSmiteWeapon.ResetCoolRate();
            containedSmiteWeapon = null;
        }
    }

    private void Update()
    {
        if (!Utilities.IsValid(containedSmiteWeapon))
            return;

        if (!Networking.IsOwner(containedSmiteWeapon.gameObject)) return;

        if (containedSmiteWeapon.currentHeat <= 0f)
        {
            if (containedSmiteWeapon.hitCount > 0)
                containedSmiteWeapon.EvaluateQuality();
            containedSmiteWeapon.ResetCoolRate();
            containedSmiteWeapon = null;
        }
    }
}
