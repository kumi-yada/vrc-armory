
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Racket : UdonSharpBehaviour
{
    private Storage storage;

    private void FindStorage()
    {
        storage = Find(Networking.LocalPlayer);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(storage)) return;

        SmiteWeapon weapon = other.GetComponent<SmiteWeapon>();
        storage.StoreItem(weapon);
    }

    private Storage Find(VRCPlayerApi player)
    {
        var objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            Storage foundScript = objects[i].GetComponentInChildren<Storage>();
            if (Utilities.IsValid(foundScript)) return foundScript;
        }
        return null;
    }
}
