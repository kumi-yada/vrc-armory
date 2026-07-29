using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Tongs : UdonSharpBehaviour
{
    public Transform attachPoint;

    private SmiteWeapon heldSmiteWeapon;
    private SmiteWeapon nearbySmiteWeapon;

    private void OnTriggerEnter(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (Utilities.IsValid(weapon))
            nearbySmiteWeapon = weapon;
    }

    private void OnTriggerExit(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (weapon != null && weapon == nearbySmiteWeapon)
            nearbySmiteWeapon = null;
    }

    public void GrabSmiteWeapon(SmiteWeapon weapon)
    {
        if (!Utilities.IsValid(weapon)) return;
        if (heldSmiteWeapon != null) return;

        nearbySmiteWeapon = weapon;
        heldSmiteWeapon = weapon;
        weapon.OnGrabbed();
    }

    public override void OnPickupUseDown()
    {
        if (Utilities.IsValid(heldSmiteWeapon))
        {
            SmitePoint activePoint = heldSmiteWeapon.GetActiveSmitePoint();
            if (Utilities.IsValid(activePoint) && activePoint.IsActive && !activePoint.IsFinished)
            {
                activePoint.CheckHit();
                return;
            }

            heldSmiteWeapon.OnReleased();
            heldSmiteWeapon = null;
            return;
        }

        if (Utilities.IsValid(nearbySmiteWeapon))
        {
            heldSmiteWeapon = nearbySmiteWeapon;
            nearbySmiteWeapon.OnGrabbed();
        }
    }
}
