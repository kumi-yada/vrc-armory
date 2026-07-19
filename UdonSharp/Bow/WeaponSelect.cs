using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WeaponSelect : UdonSharpBehaviour
{
    void Start()
    {

    }

    public override void Interact()
    {
        var weapon = Find(Networking.LocalPlayer);
        if (weapon != null)
        {
            weapon.SetWeapon();
        }
    }
    public PlayerWeapon Find(VRCPlayerApi player)
    {
        var objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            PlayerWeapon foundScript = objects[i].GetComponentInChildren<PlayerWeapon>();
            if (Utilities.IsValid(foundScript)) return foundScript;
        }
        return null;
    }


}
