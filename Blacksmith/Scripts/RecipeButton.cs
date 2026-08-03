
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public class RecipeButton : UdonSharpBehaviour
{
    [SerializeField] public SmiteWeapon recipe;
    [SerializeField] private int minimumLevel = 1;
    [SerializeField] private int minimumForgeLevel = 1;
    [SerializeField] private Forge forge;

    private Button button;
    private TextMeshProUGUI label;
    private int recipeIndex = -1;

    void Start()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<TextMeshProUGUI>();

        if (recipe != null && label != null)
            label.text = recipe.recipeName;

        if (forge != null)
        {
            for (int i = 0; i < forge.ItemCount; i++)
            {
                if (forge.GetItemByIndex(i) == recipe)
                {
                    recipeIndex = i;
                    break;
                }
            }
        }

        UpdateButtonState();
    }

    public void OnClick()
    {
        if (forge == null || recipeIndex < 0) return;
        forge.SelectRecipe(recipeIndex);
    }

    private void UpdateButtonState()
    {
        if (button == null) return;
        if (Networking.LocalPlayer == null) return;

        float exp = PlayerData.GetFloat(Networking.LocalPlayer, BlacksmithData.EXP_KEY);
        int level = BlacksmithData.GetLevel(exp);
        float forgeExp = PlayerData.GetFloat(Networking.LocalPlayer, BlacksmithData.FORGE_LEVEL_KEY);
        int forgeLevel = Mathf.Max(1, (int)forgeExp);
        button.interactable = level >= minimumLevel && forgeLevel >= minimumForgeLevel;
    }
}
