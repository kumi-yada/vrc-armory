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
        Debug.Log("Tongs: OnTriggerEnter with " + (weapon != null ? weapon.recipeName : "null"));
        if (Utilities.IsValid(weapon))
            nearbySmiteWeapon = weapon;
    }

    private void OnTriggerExit(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (weapon != null && weapon == nearbySmiteWeapon)
            nearbySmiteWeapon = null;
    }

    public override void OnPickupUseDown()
    {
        if (Utilities.IsValid(heldSmiteWeapon))
        {
            Debug.Log("Tongs: UseDown on held smite weapon: " + heldSmiteWeapon.recipeName);
            SmitePoint activePoint = heldSmiteWeapon.GetActiveSmitePoint();
            if (Utilities.IsValid(activePoint) && activePoint.CanHit())
            {
                activePoint.CheckHit();
                Debug.Log("Tongs: Hit active smite point on held smite weapon: " + heldSmiteWeapon.recipeName);
                return;
            }

            heldSmiteWeapon.OnReleased();
            heldSmiteWeapon = null;
            Debug.Log("Tongs: Released held smite weapon");
            return;
        }

        if (Utilities.IsValid(nearbySmiteWeapon))
        {
            heldSmiteWeapon = nearbySmiteWeapon;
            nearbySmiteWeapon.OnGrabbed();
            Debug.Log("Tongs: Grabbed nearby smite weapon: " + nearbySmiteWeapon.recipeName);
        }
    }
}
