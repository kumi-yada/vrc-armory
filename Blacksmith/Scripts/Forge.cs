using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Forge : UdonSharpBehaviour
{
    [Header("Items")]
    [SerializeField] private GameObject[] spawnItems;
    [SerializeField] private Transform itemSpawnPoint;

    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private GameObject recipeListPage;
    [SerializeField] private GameObject recipeDetailsPage;
    [SerializeField] private TextMeshProUGUI recipeNameText;

    [Header("Interaction")]
    [SerializeField] private float hideUIDistance = 3f;

    private int selectedRecipeIndex = -1;
    private GameObject currentItem;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;

        recipeListPage.SetActive(true);
        recipeDetailsPage.SetActive(false);
    }

    void Update()
    {
        if (!Utilities.IsValid(uiCanvas)) return;
        if (!uiCanvas.enabled) return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!Utilities.IsValid(localPlayer)) return;

        if (Vector3.Distance(owner.GetPosition(), transform.position) <= hideUIDistance) return;

        if (!Utilities.IsValid(uiCanvas)) return;
        uiCanvas.enabled = false;
    }

    public void SelectRecipe(int index)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (Utilities.IsValid(currentItem)) return;

        selectedRecipeIndex = index;
        SpawnSmiteWeapon();

        recipeListPage.SetActive(false);
        recipeDetailsPage.SetActive(true);

        SmiteWeapon weapon = currentItem.GetComponent<SmiteWeapon>();
        if (Utilities.IsValid(recipeNameText) && Utilities.IsValid(weapon))
            recipeNameText.text = weapon.recipeName;
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = !uiCanvas.enabled;
    }

    public void CancelSelection()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (Utilities.IsValid(currentItem))
            currentItem.SetActive(false);
        currentItem = null;

        recipeDetailsPage.SetActive(false);
        recipeListPage.SetActive(true);
    }

    public void SpawnSmiteWeapon()
    {
        if (spawnItems == null || selectedRecipeIndex < 0 || selectedRecipeIndex >= spawnItems.Length)
            return;

        if (Utilities.IsValid(currentItem))
            currentItem.SetActive(false);

        currentItem = spawnItems[selectedRecipeIndex];
        if (!Utilities.IsValid(currentItem))
            return;

        Vector3 pos = Utilities.IsValid(itemSpawnPoint)
            ? itemSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        currentItem.transform.position = pos;
        currentItem.transform.rotation = Quaternion.identity;
        currentItem.SetActive(true);
        Networking.SetOwner(Networking.LocalPlayer, currentItem);
    }
}
