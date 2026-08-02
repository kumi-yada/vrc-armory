
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class ImprovedPickup : UdonSharpBehaviour
{
    void Start()
    {
        var pickup = GetComponent<VRCPickup>();
        pickup.pickupable = Networking.IsOwner(gameObject);

        var owner = Networking.GetOwner(gameObject);
        if (owner.IsUserInVR())
        {
            pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            pickup.orientation = VRC_Pickup.PickupOrientation.Any;
        }
        else
        {
            pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            pickup.orientation = VRC_Pickup.PickupOrientation.Grip;
        }
    }

}
