using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public class Shop : UdonSharpBehaviour
{
    [Header("References")]
    [SerializeField] private Forge forge;
    [SerializeField] private Transform displayTransform;

    [System.NonSerialized] private Storage cachedStorage;

    void Start()
    {
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;

        Storage storage = FindPlayerStorage(player);
        if (!Utilities.IsValid(storage)) return;

        cachedStorage = storage;
        storage.gameObject.SetActive(true);

        if (Utilities.IsValid(displayTransform))
            storage.transform.SetPositionAndRotation(displayTransform.position, displayTransform.rotation);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;

        if (Utilities.IsValid(cachedStorage))
            cachedStorage.gameObject.SetActive(false);
    }

    public void SellItem(int slotIndex)
    {
        if (!Utilities.IsValid(cachedStorage)) return;
        InventorySlot[] slots = cachedStorage.slots;
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length) return;
        InventorySlot slot = slots[slotIndex];
        if (!Utilities.IsValid(slot) || slot.itemIndex == -1) return;

        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(slot.itemIndex) : null;
        if (!Utilities.IsValid(weapon)) return;

        float price = weapon.baseSellPrice * (1f + slot.quality);

        float currentGold = PlayerData.GetFloat(Networking.LocalPlayer, BlacksmithData.GOLD_KEY);
        PlayerData.SetFloat(BlacksmithData.GOLD_KEY, currentGold + price);
        Debug.Log("Shop: SellItem: slot=" + slotIndex + " item=" + weapon.recipeName + " price=" + price + " newGold=" + (currentGold + price));

        slot.Clear();
    }

    private Storage FindPlayerStorage(VRCPlayerApi player)
    {
        GameObject[] objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            Storage storage = objects[i].GetComponent<Storage>();
            if (Utilities.IsValid(storage)) return storage;
        }
        return null;
    }
}
