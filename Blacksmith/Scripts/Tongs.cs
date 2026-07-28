using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class Tongs : UdonSharpBehaviour
{
    public Transform attachPoint;
    public Anvil anvil;

    private Weapon heldWeapon;
    private Weapon nearbyWeapon;

    private void OnTriggerEnter(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (Utilities.IsValid(weapon))
            nearbyWeapon = weapon;
    }

    private void OnTriggerExit(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (weapon != null && weapon == nearbyWeapon)
            nearbyWeapon = null;
    }

    public void GrabWeapon(Weapon weapon)
    {
        if (!Utilities.IsValid(weapon)) return;
        if (heldWeapon != null) return;

        nearbyWeapon = weapon;
        heldWeapon = weapon;
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

        if (IsHoldingWeapon())
        {
            heldWeapon.OnReleased();
            heldWeapon = null;
        }
        else if (Utilities.IsValid(nearbyWeapon))
        {
            Networking.SetOwner(Networking.LocalPlayer, nearbyWeapon.gameObject);
            heldWeapon = nearbyWeapon;
            nearbyWeapon.OnGrabbed();
        }
    }

    public bool IsHoldingWeapon()
    {
        return heldWeapon != null;
    }
}
