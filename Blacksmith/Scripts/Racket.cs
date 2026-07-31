
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Racket : UdonSharpBehaviour
{
    [SerializeField] private Forge forge;
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

        if (weapon.isStored)
        {
            weapon.gameObject.SetActive(false);
            return;
        }

        int idx = weapon.spawnItemIndex;
        if (idx < 0 || idx >= forge.ItemCount) return;

        for (int i = 0; i < storage.slots.Length; i++)
        {
            if (storage.slots[i].itemIndex != -1) continue;
            storage.slots[i].SetItem(idx, weapon.qualityScore, weapon.recipeName, weapon.finishTimeMs);
            weapon.gameObject.SetActive(false);
            return;
        }
    }
}
