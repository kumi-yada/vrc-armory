using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Storage : UdonSharpBehaviour
{
    [SerializeField] private Forge forge;
    [SerializeField] public InventorySlot[] slots;

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
            weapon.isStored = true;
            return;
        }
    }
}
