using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class Tongs : UdonSharpBehaviour
{
    public Transform attachPoint;
    public Anvil anvil;

    private SmithWeapon heldSmithWeapon;
    private SmithWeapon nearbySmithWeapon;

    private void OnTriggerEnter(Collider other)
    {
        SmithWeapon weapon = other.GetComponentInParent<SmithWeapon>();
        if (Utilities.IsValid(weapon))
            nearbySmithWeapon = weapon;
    }

    private void OnTriggerExit(Collider other)
    {
        SmithWeapon weapon = other.GetComponentInParent<SmithWeapon>();
        if (weapon != null && weapon == nearbySmithWeapon)
            nearbySmithWeapon = null;
    }

    public void GrabSmithWeapon(SmithWeapon weapon)
    {
        if (!Utilities.IsValid(weapon)) return;
        if (heldSmithWeapon != null) return;

        nearbySmithWeapon = weapon;
        heldSmithWeapon = weapon;
        weapon.OnGrabbed();
    }

    public override void OnPickupUseDown()
    {
        if (anvil?.ActiveSmitePoint != null)
        {
            SmitePoint smite = anvil.ActiveSmitePoint;
            if (!smite.IsActive || smite.IsFinished) return;

            smite.CheckHit();
            return;
        }

        if (IsHoldingSmithWeapon())
        {
            heldSmithWeapon.OnReleased();
            heldSmithWeapon = null;
        }
        else if (Utilities.IsValid(nearbySmithWeapon))
        {
            Networking.SetOwner(Networking.LocalPlayer, nearbySmithWeapon.gameObject);
            heldSmithWeapon = nearbySmithWeapon;
            nearbySmithWeapon.OnGrabbed();
        }
    }

    public bool IsHoldingSmithWeapon()
    {
        return heldSmithWeapon != null;
    }
}
