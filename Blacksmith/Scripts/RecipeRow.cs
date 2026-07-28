using UdonSharp;
using UnityEngine;
using TMPro;

public class RecipeRow : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI detailText;

    private Forge forge;
    private int recipeIndex;

    public void Setup(Forge owner, int index)
    {
        forge = owner;
        recipeIndex = index;
    }

    public void SetDisplay(string name, string details, bool selected)
    {
        if (nameText != null)
        {
            if (selected)
                nameText.text = "<color=yellow>" + name + "</color>";
            else
                nameText.text = name;
        }
        if (detailText != null)
            detailText.text = details;
    }

    public override void Interact()
    {
        if (forge != null)
            forge.SelectRecipe(recipeIndex);
    }
}
