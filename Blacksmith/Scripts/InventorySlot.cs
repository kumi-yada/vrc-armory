using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InventorySlot : UdonSharpBehaviour
{
    [UdonSynced] public int itemIndex = -1;
    [UdonSynced] public float quality;
    [UdonSynced] public int finishTimeMs;

    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI qualityNameText;
    [SerializeField] private TextMeshProUGUI finishDateText;
    [SerializeField] private Forge forge;

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

        if (recipeNameText != null)
            recipeNameText.text = "";
        if (qualityNameText != null)
            qualityNameText.text = "";
        if (finishDateText != null)
            finishDateText.text = "";

        RequestSerialization();
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
            return;
        }

        if (recipeNameText != null)
        {
            string rn = "";
            if (forge != null)
            {
                var weapon = forge.GetItemByIndex(itemIndex);
                if (weapon != null)
                    rn = weapon.recipeName;
            }
            recipeNameText.text = rn;
        }

        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(quality);
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
