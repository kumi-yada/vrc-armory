
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Racket : UdonSharpBehaviour
{
    private Storage storage;

    private void FindStorage()
    {
        GameObject[] playerObjects = Networking.LocalPlayer.GetPlayerObjects();
        foreach (GameObject po in playerObjects)
        {
            storage = po.GetComponentInChildren<Storage>();
            if (Utilities.IsValid(storage)) return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (!Utilities.IsValid(storage)) FindStorage();
        if (!Utilities.IsValid(storage)) return;

        SmiteWeapon weapon = other.GetComponent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon)) return;
        if (!weapon.isCompleted) return;
        if (!Networking.IsOwner(weapon.gameObject)) return;

        storage.StoreItem(weapon);
    }
}
