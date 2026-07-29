using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Storage : UdonSharpBehaviour
{
    [SerializeField] private Forge forge;
    [SerializeField] private InventorySlot[] slots;

    public void AutoStoreItem(SmiteWeapon weapon)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(weapon)) return;

        int idx = weapon.spawnItemIndex;
        if (idx < 0 || idx >= forge.spawnItems.Length) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemIndex != -1) continue;
            slots[i].SetItem(idx, weapon.qualityScore, weapon.recipeName);
            weapon.isStored = true;
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;

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
        if (idx < 0 || idx >= forge.spawnItems.Length) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemIndex != -1) continue;
            slots[i].SetItem(idx, weapon.qualityScore, weapon.recipeName);
            weapon.gameObject.SetActive(false);
            return;
        }
    }
}
