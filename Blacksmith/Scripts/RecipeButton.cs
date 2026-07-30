
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public class RecipeButton : UdonSharpBehaviour
{
    [SerializeField] public SmiteWeapon recipe;
    [SerializeField] private int minimumLevel = 1;
    [SerializeField] private Forge forge;

    private Button button;
    private Text label;
    private int recipeIndex = -1;

    void Start()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<Text>();

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
            Debug.Log("RecipeButton: found recipe '" + (recipe != null ? recipe.recipeName : "null") + "' at index " + recipeIndex);
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
        button.interactable = level >= minimumLevel;
    }
}
