using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Forge : UdonSharpBehaviour
{
    [Header("Data")]
    [SerializeField] private RecipeData[] recipes;

    [Header("Pool")]
    [SerializeField] private VRCObjectPool itemPool;

    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Transform recipeContainer;

    [Header("Spawn")]
    [SerializeField] private Transform itemSpawnPoint;

    private RecipeRow[] recipeRows;
    private string[] recipeNames;
    private string[] recipeDetails;
    private int selectedRecipeIndex = -1;
    private bool selectingRecipe;
    private GameObject currentItem;
    private Weapon currentWeapon;
    private RecipeData currentRecipe;
    private bool isActive;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;

        if (recipeRows == null || recipeRows.Length == 0)
        {
            if (Utilities.IsValid(recipeContainer))
                recipeRows = recipeContainer.GetComponentsInChildren<RecipeRow>(true);
            else
                recipeRows = GetComponentsInChildren<RecipeRow>(true);
        }

        recipeNames = new string[recipeRows.Length];
        recipeDetails = new string[recipeRows.Length];

        for (int i = 0; i < recipeRows.Length; i++)
        {
            RecipeRow row = recipeRows[i];
            if (!Utilities.IsValid(row))
                continue;

            row.Setup(this, i);

            if (i < recipes.Length)
            {
                RecipeData r = recipes[i];
                recipeNames[i] = r.recipeName;
                recipeDetails[i] = "Heat: " + r.optimalFormingHeat + " °C\nResist: " + r.heatResistance;
                row.SetDisplay(recipeNames[i], recipeDetails[i], false);
            }

            row.gameObject.SetActive(false);
        }
    }

    public void SelectRecipe(int index)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!selectingRecipe) return;

        selectedRecipeIndex = index;
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (isActive || selectingRecipe)
            return;

        selectingRecipe = true;
        selectedRecipeIndex = 0;
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = true;
    }

    public void ConfirmRecipe()
    {
        if (!Networking.IsOwner(gameObject)) return;
        selectingRecipe = false;
        HideUI();

        if (selectedRecipeIndex < recipes.Length)
            currentRecipe = recipes[selectedRecipeIndex];

        SpawnItem();
        SetActive(true);
    }

    public void CancelSelection()
    {
        if (!Networking.IsOwner(gameObject)) return;
        selectingRecipe = false;
        HideUI();
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
    }

    private void HideUI()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (Utilities.IsValid(currentWeapon))
            currentWeapon.isHeated = active;
    }

    public void SpawnWeapon()
    {
        if (!Utilities.IsValid(itemPool))
            return;

        currentItem = itemPool.TryToSpawn();
        if (!Utilities.IsValid(currentItem))
            return;

        currentWeapon = currentItem.GetComponent<Weapon>();
        if (Utilities.IsValid(currentWeapon))
        {
            currentWeapon.recipe = currentRecipe;
            currentWeapon.isHeated = isActive;
        }

        Vector3 pos = Utilities.IsValid(itemSpawnPoint)
            ? itemSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        currentItem.transform.position = pos;
        currentItem.transform.rotation = Quaternion.identity;
    }

    private void SpawnItem()
    {
        SpawnWeapon();
    }
}
