using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    private Weapon containedWeapon;

    public void OnTriggerEnter(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (!Utilities.IsValid(weapon))
            return;

        containedWeapon = weapon;
        containedWeapon.isHeated = false;
        containedWeapon.SetCoolRate(coolRate);
    }

    public void OnTriggerExit(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (!Utilities.IsValid(weapon))
            return;

        if (containedWeapon == weapon)
        {
            containedWeapon.ResetCoolRate();
            containedWeapon = null;
        }
    }

    private void Update()
    {
        if (!Utilities.IsValid(containedWeapon))
            return;

        if (containedWeapon.GetHeat() <= 0f)
        {
            if (containedWeapon.GetHitCount() > 0)
                containedWeapon.EvaluateQuality();
            containedWeapon.ResetCoolRate();
            containedWeapon = null;
        }
    }
}
