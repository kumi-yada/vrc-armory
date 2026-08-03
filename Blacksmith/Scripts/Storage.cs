using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Storage : UdonSharpBehaviour
{
    [SerializeField] private Forge forge;
    [SerializeField] private TextMeshProUGUI slotCounterText;

    [System.NonSerialized] public string currentMode = MODE_STASH;
    [System.NonSerialized] public InventorySlot[] slots;
    public const string MODE_STASH = "stash";
    public const string MODE_SELL = "sell";

    private void Start()
    {
        slots = GetComponentsInChildren<InventorySlot>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            slots[i].slotIndex = i;
            slots[i].Refresh();
        }
        RefreshSlotCounter();
    }

    public void RefreshSlotCounter()
    {
        if (slotCounterText == null) return;
        int used = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i].itemIndex != -1) used++;
        }
        slotCounterText.text = used + " / " + slots.Length;
    }

    public void SetMode(string mode)
    {
        currentMode = mode;
        UpdateSlots();
        RefreshSlotCounter();
    }

    private void UpdateSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            slots[i].Refresh();
        }
    }

    public void StashOtherSlots(InventorySlot except)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!Utilities.IsValid(slots[i])) continue;
            if (slots[i] == except) continue;
            slots[i].Stash();
        }

        Debug.Log("Storage: StashOtherSlots: except slotIndex = " + (except != null ? except.slotIndex.ToString() : "null"));
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
            weapon.activeSlot = slots[i];
            slots[i].StoreItem(idx, weapon.qualityScore, weapon.recipeName, weapon.finishTimeMs);
            RefreshSlotCounter();
            return;
        }
    }

    public void StoreItem(SmiteWeapon weapon)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(weapon)) return;
        if (!weapon.isCompleted) return;

        InventorySlot existing = weapon.activeSlot;
        if (Utilities.IsValid(existing))
        {
            existing.Stash();
            return;
        }

        Debug.Log("Storage: trying to store an item which is not in any slot: recipeName = " + weapon.recipeName + ", spawnItemIndex = " + weapon.spawnItemIndex);
    }
}
