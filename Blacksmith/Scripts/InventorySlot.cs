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

    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI qualityNameText;
    [SerializeField] private Forge forge;

    public void SetItem(int index, float q, string recipeName)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        itemIndex = index;
        quality = q;

        if (recipeNameText != null)
            recipeNameText.text = recipeName;
        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(q);

        RequestSerialization();
    }

    public void Clear()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        itemIndex = -1;
        quality = 0f;

        if (recipeNameText != null)
            recipeNameText.text = "";
        if (qualityNameText != null)
            qualityNameText.text = "";

        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        if (itemIndex == -1)
        {
            if (recipeNameText != null) recipeNameText.text = "";
            if (qualityNameText != null) qualityNameText.text = "";
            return;
        }

        if (recipeNameText != null)
        {
            string rn = "";
            if (forge != null && itemIndex >= 0 && itemIndex < forge.spawnItems.Length)
            {
                var weapon = forge.spawnItems[itemIndex].GetComponent<SmiteWeapon>();
                if (weapon != null)
                    rn = weapon.recipeName;
            }
            recipeNameText.text = rn;
        }

        if (qualityNameText != null)
            qualityNameText.text = GetQualityLabel(quality);
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
}
