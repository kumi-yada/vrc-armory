using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    private SmithWeapon containedSmithWeapon;

    public void OnTriggerEnter(Collider other)
    {
        SmithWeapon weapon = other.GetComponentInParent<SmithWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        containedSmithWeapon = weapon;
        containedSmithWeapon.isHeated = false;
        containedSmithWeapon.SetCoolRate(coolRate);
    }

    public void OnTriggerExit(Collider other)
    {
        SmithWeapon weapon = other.GetComponentInParent<SmithWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        if (containedSmithWeapon == weapon)
        {
            containedSmithWeapon.ResetCoolRate();
            containedSmithWeapon = null;
        }
    }

    private void Update()
    {
        if (!Utilities.IsValid(containedSmithWeapon))
            return;

        if (containedSmithWeapon.GetHeat() <= 0f)
        {
            if (containedSmithWeapon.GetHitCount() > 0)
                containedSmithWeapon.EvaluateQuality();
            containedSmithWeapon.ResetCoolRate();
            containedSmithWeapon = null;
        }
    }
}
