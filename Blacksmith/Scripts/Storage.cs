using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Storage : UdonSharpBehaviour
{
    [SerializeField] private Forge forge;
    [SerializeField] public InventorySlot[] slots;

    public const string MODE_SELL = "sell";
    public const string MODE_STASH = "stash";

    [System.NonSerialized] public string currentMode = MODE_STASH;

    public void SetMode(string mode)
    {
        currentMode = mode;
    }

    public bool IsItemStored(int idx)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i].itemIndex == idx) return true;
        }
        return false;
    }

    public void StashOtherSlots(InventorySlot except)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i] == except) continue;
            slots[i].Stash();
        }
    }

    public void AutoStoreItem(SmiteWeapon weapon)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(weapon)) return;

        int idx = weapon.spawnItemIndex;
        if (idx < 0 || idx >= forge.ItemCount) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i].itemIndex != -1) continue;
            slots[i].SetItem(idx, weapon.qualityScore, weapon.recipeName, weapon.finishTimeMs);
            slots[i].isShown = true;
            slots[i].RequestSerialization();
            return;
        }
    }

    public void StoreItem(SmiteWeapon weapon)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(weapon)) return;

        int idx = weapon.spawnItemIndex;
        if (idx < 0 || idx >= forge.ItemCount) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i].itemIndex != -1) continue;
            slots[i].SetItem(idx, weapon.qualityScore, weapon.recipeName, weapon.finishTimeMs);
            slots[i].isShown = false;
            slots[i].RequestSerialization();
            return;
        }
    }
}
