using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InventorySlot : UdonSharpBehaviour
{
    [UdonSynced] public int itemIndex = -1;
    [UdonSynced] public float quality;
    [UdonSynced] public int finishTimeMs;

    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI qualityNameText;
    [SerializeField] private TextMeshProUGUI finishDateText;
    [SerializeField] private TextMeshProUGUI sellPriceText;
    [SerializeField] private TextMeshProUGUI displayStatusText;
    [SerializeField] private Image weaponIconImage;
    [System.NonSerialized] public int slotIndex;

    public void StoreItem(int index, float q, string recipeName, int timeMs)
    {
        if (!Networking.IsOwner(gameObject)) return;

        itemIndex = index;
        quality = q;
        finishTimeMs = timeMs;

        if (recipeNameText != null)
            recipeNameText.text = recipeName;
        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(q);
        UpdateFinishDateText();

        Forge forge = GetForge();
        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(index) : null;
        if (weaponIconImage != null)
            weaponIconImage.sprite = weapon != null ? weapon.weaponIcon : null;

        RequestSerialization();
    }

    public void Clear()
    {
        if (!Networking.IsOwner(gameObject)) return;

        itemIndex = -1;
        quality = 0f;
        finishTimeMs = 0;

        if (recipeNameText != null)
            recipeNameText.text = "";
        if (qualityNameText != null)
            qualityNameText.text = "";
        if (finishDateText != null)
            finishDateText.text = "";
        if (sellPriceText != null)
        {
            sellPriceText.text = "";
            sellPriceText.gameObject.SetActive(false);
        }
        if (displayStatusText != null)
        {
            displayStatusText.text = "";
            displayStatusText.gameObject.SetActive(false);
        }
        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = null;
            weaponIconImage.gameObject.SetActive(false);
        }

        RequestSerialization();
    }

    public void OnClick()
    {
        Debug.Log("InventorySlot: OnClick: slot=" + slotIndex + " itemIndex=" + itemIndex);
        if (itemIndex == -1) return;
        Storage storage = GetStorage();
        if (storage != null && storage.currentMode == Storage.MODE_SELL)
            SellItem();
        else
            ToggleStash();
    }

    public void ToggleStash()
    {
        if (itemIndex == -1) return;
        Forge forge = GetForge();
        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(itemIndex) : null;
        if (!Utilities.IsValid(weapon)) return;
        if (IsCurrentSlotDisplayed(weapon))
        {
            weapon.Hide();
            RefreshUI();
        }
        else
        {
            DisplayWeapon();
        }
    }

    private bool IsCurrentSlotDisplayed(SmiteWeapon weapon)
    {
        if (itemIndex == -1) return false;
        if (!Utilities.IsValid(weapon)) return false;
        if (weapon.activeSlot != null && weapon.activeSlot.slotIndex != slotIndex) return false;
        return weapon.isDisplayed;
    }

    public void Stash()
    {
        if (itemIndex == -1) return;
        Forge forge = GetForge();
        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(itemIndex) : null;
        if (!Utilities.IsValid(weapon)) return;
        weapon.Hide();
        RefreshUI();
    }

    private void DisplayWeapon()
    {
        Debug.Log("InventorySlot: DisplayWeapon: slot=" + slotIndex);
        Forge forge = GetForge();
        if (forge == null) return;
        SmiteWeapon weapon = forge.GetItemByIndex(itemIndex);
        if (!Utilities.IsValid(weapon)) return;

        Storage storage = GetStorage();
        if (storage != null)
            storage.StashOtherSlots(this);

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        Vector3 forward = localPlayer.GetRotation() * Vector3.forward;
        weapon.transform.position = localPlayer.GetPosition() + forward * 0.7f + Vector3.up * 1.5f;
        weapon.transform.rotation = Quaternion.identity;
        weapon.Show(this);
        UpdateDisplayStatus(true);
    }

    public void SellItem()
    {
        if (itemIndex == -1) return;

        Forge forge = GetForge();
        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(itemIndex) : null;
        if (!Utilities.IsValid(weapon)) return;

        int price = Mathf.CeilToInt(weapon.baseSellPrice * (1f + quality));
        int currentGold = PlayerData.GetInt(Networking.LocalPlayer, BlacksmithData.GOLD_KEY);
        PlayerData.SetInt(BlacksmithData.GOLD_KEY, currentGold + price);
        Debug.Log("InventorySlot: SellItem: slot=" + slotIndex + " item=" + weapon.recipeName + " price=" + price + " newGold=" + (currentGold + price));

        Clear();
    }

    private void Start()
    {
        RefreshUI();
    }

    public override void OnDeserialization()
    {
        RefreshUI();
    }

    private void EmptyUI()
    {
        if (recipeNameText != null) recipeNameText.text = "";
        if (qualityNameText != null) qualityNameText.text = "";
        if (finishDateText != null) finishDateText.text = "";
        if (sellPriceText != null)
        {
            sellPriceText.text = "";
            sellPriceText.gameObject.SetActive(false);
        }
        if (displayStatusText != null)
        {
            displayStatusText.text = "";
            displayStatusText.gameObject.SetActive(false);
        }
        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = null;
            weaponIconImage.gameObject.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (itemIndex == -1)
        {
            EmptyUI();
            return;
        }

        SmiteWeapon weapon = null;
        Forge forge = GetForge();
        if (forge != null)
            weapon = forge.GetItemByIndex(itemIndex);

        if (recipeNameText != null)
            recipeNameText.text = weapon != null ? weapon.recipeName : "";

        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(quality);

        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = weapon != null ? weapon.weaponIcon : null;
            weaponIconImage.gameObject.SetActive(weapon != null && weapon.weaponIcon != null);
        }

        Storage storage = GetStorage();
        bool sellMode = storage != null && storage.currentMode == Storage.MODE_SELL;

        if (sellPriceText != null)
        {
            float price = weapon != null ? weapon.baseSellPrice * (1f + quality) : 0f;
            bool showPrice = sellMode && price > 0f;
            sellPriceText.text = showPrice ? Mathf.CeilToInt(price) + "g" : "";
            sellPriceText.gameObject.SetActive(showPrice);
        }

        UpdateFinishDateText();
        UpdateDisplayStatus(weapon != null && weapon.activeSlot != null && weapon.activeSlot.slotIndex == slotIndex);
    }

    private void UpdateDisplayStatus(bool displayed)
    {
        if (displayStatusText == null) return;
        if (itemIndex == -1)
        {
            displayStatusText.text = "";
            displayStatusText.gameObject.SetActive(false);
            return;
        }
        displayStatusText.text = displayed ? "On display" : "";
        displayStatusText.gameObject.SetActive(displayed);
    }

    private string GetQualityLabel(float q)
    {
        if (itemIndex == -1) return "";
        if (q >= 0.9f) return "Perfect";
        if (q >= 0.7f) return "Excellent";
        if (q >= 0.5f) return "Good";
        if (q >= 0.2f) return "Fair";
        return "Poor";
    }

    private void UpdateFinishDateText()
    {
        if (finishDateText == null) return;
        Storage storage = GetStorage();
        if (storage != null && storage.currentMode == Storage.MODE_SELL)
        {
            finishDateText.text = "";
            return;
        }
        finishDateText.text = finishTimeMs > 0 ? "Finished: " + FormatElapsed(finishTimeMs) : "";
    }

    public void Refresh()
    {
        RefreshUI();
    }

    private Forge GetForge()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return null;

        GameObject[] playerObjects = Networking.GetPlayerObjects(local);
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (!Utilities.IsValid(playerObjects[i])) continue;
            Forge forge = playerObjects[i].GetComponentInChildren<Forge>();
            if (Utilities.IsValid(forge)) return forge;
        }

        return null;
    }

    private Storage GetStorage()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return null;

        GameObject[] playerObjects = Networking.GetPlayerObjects(local);
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (!Utilities.IsValid(playerObjects[i])) continue;
            Storage storage = playerObjects[i].GetComponentInChildren<Storage>();
            if (Utilities.IsValid(storage)) return storage;
        }

        return null;
    }

    private string FormatElapsed(int finishMs)
    {
        int elapsedMs = Networking.GetServerTimeInMilliseconds() - finishMs;
        if (elapsedMs < 0) elapsedMs = 0;
        int totalSec = elapsedMs / 1000;

        if (totalSec < 600) return "recently";
        int totalMin = totalSec / 60;
        if (totalMin < 60) return totalMin + "m ago";
        int hours = totalMin / 60;
        int mins = totalMin % 60;
        return hours + "h " + mins + "m ago";
    }
}
