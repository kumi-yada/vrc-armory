using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InventorySlot : UdonSharpBehaviour
{
    [UdonSynced] public int itemIndex = -1;
    [UdonSynced] public float quality;
    [UdonSynced] public int finishTimeMs;
    [UdonSynced] public bool isShown;

    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI qualityNameText;
    [SerializeField] private TextMeshProUGUI finishDateText;
    [SerializeField] private TextMeshProUGUI sellPriceText;
    [SerializeField] private Forge forge;
    [SerializeField] private Storage storage;
    [SerializeField] private int slotIndex;

    public void SetItem(int index, float q, string recipeName, int timeMs)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        itemIndex = index;
        quality = q;
        finishTimeMs = timeMs;

        if (recipeNameText != null)
            recipeNameText.text = recipeName;
        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(q);
        UpdateFinishDateText();

        RequestSerialization();
    }

    public void Clear()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        itemIndex = -1;
        quality = 0f;
        finishTimeMs = 0;
        isShown = false;

        if (recipeNameText != null)
            recipeNameText.text = "";
        if (qualityNameText != null)
            qualityNameText.text = "";
        if (finishDateText != null)
            finishDateText.text = "";
        if (sellPriceText != null)
            sellPriceText.text = "";

        RequestSerialization();
    }

    public void OnClick()
    {
        if (itemIndex == -1) return;
        if (storage != null && storage.currentMode == Storage.MODE_SELL)
            SellItem();
        else
            ToggleStash();
    }

    public void ToggleStash()
    {
        if (itemIndex == -1) return;
        isShown = !isShown;
        if (isShown)
            DisplayWeapon();
        else
            StashWeapon();
        RequestSerialization();
    }

    private void StashWeapon()
    {
        Debug.Log("InventorySlot: StashWeapon: slot=" + slotIndex);
        if (forge == null) return;
        SmiteWeapon weapon = forge.GetItemByIndex(itemIndex);
        if (!Utilities.IsValid(weapon)) return;
        if (weapon.gameObject.activeSelf)
            weapon.gameObject.SetActive(false);
    }

    public void Stash()
    {
        if (!isShown) return;
        isShown = false;
        StashWeapon();
        RequestSerialization();
    }

    private void DisplayWeapon()
    {
        Debug.Log("InventorySlot: DisplayWeapon: slot=" + slotIndex);
        if (forge == null) return;
        SmiteWeapon weapon = forge.GetItemByIndex(itemIndex);
        if (!Utilities.IsValid(weapon)) return;

        if (storage != null)
            storage.StashOtherSlots(this);

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        Vector3 forward = localPlayer.GetRotation() * Vector3.forward;
        weapon.transform.position = localPlayer.GetPosition() + forward * 1.5f + Vector3.up * 0.5f;
        weapon.transform.rotation = Quaternion.identity;
        weapon.gameObject.SetActive(true);
    }

    public void SellItem()
    {
        if (itemIndex == -1) return;

        SmiteWeapon weapon = forge != null ? forge.GetItemByIndex(itemIndex) : null;
        if (!Utilities.IsValid(weapon)) return;

        float price = weapon.baseSellPrice * (1f + quality);

        float currentGold = PlayerData.GetFloat(Networking.LocalPlayer, BlacksmithData.GOLD_KEY);
        PlayerData.SetFloat(BlacksmithData.GOLD_KEY, currentGold + price);
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

    private void RefreshUI()
    {
        if (itemIndex == -1)
        {
            if (recipeNameText != null) recipeNameText.text = "";
            if (qualityNameText != null) qualityNameText.text = "";
            if (finishDateText != null) finishDateText.text = "";
            if (sellPriceText != null) sellPriceText.text = "";
            return;
        }

        SmiteWeapon weapon = null;
        if (forge != null)
            weapon = forge.GetItemByIndex(itemIndex);

        if (Utilities.IsValid(weapon))
            weapon.gameObject.SetActive(isShown);

        if (recipeNameText != null)
            recipeNameText.text = weapon != null ? weapon.recipeName : "";

        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(quality);

        if (sellPriceText != null)
        {
            float price = weapon != null ? weapon.baseSellPrice * (1f + quality) : 0f;
            sellPriceText.text = price > 0f ? Mathf.RoundToInt(price) + "g" : "";
        }

        UpdateFinishDateText();
    }

    private string GetQualityLabel(float q)
    {
        if (itemIndex == -1) return "";
        if (q >= 0.9f) return "Masterwork";
        if (q >= 0.75f) return "Excellent";
        if (q >= 0.6f) return "Good";
        if (q >= 0.4f) return "Fair";
        if (q >= 0.2f) return "Poor";
        return "Ruined";
    }

    private void UpdateFinishDateText()
    {
        if (finishDateText == null) return;
        finishDateText.text = finishTimeMs > 0 ? "Finished: " + FormatElapsed(finishTimeMs) : "";
    }

    private string FormatElapsed(int finishMs)
    {
        int elapsedMs = Networking.GetServerTimeInMilliseconds() - finishMs;
        if (elapsedMs < 0) elapsedMs = 0;
        int totalSec = elapsedMs / 1000;

        if (totalSec < 60) return totalSec + "s ago";
        int totalMin = totalSec / 60;
        if (totalMin < 60) return totalMin + "m ago";
        int hours = totalMin / 60;
        int mins = totalMin % 60;
        return hours + "h " + mins + "m ago";
    }
}
