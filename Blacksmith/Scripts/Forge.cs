using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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
    [SerializeField] private float hideUIDistance = 3f;

    [Header("Heat")]
    [SerializeField] private GameObject heatArea;
    [SerializeField] private ParticleSystem particleField;

    private int selectedRecipeIndex = -1;
    private GameObject currentItem;
    [UdonSynced] private bool isActive;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;

        recipeListPage.SetActive(true);
        recipeDetailsPage.SetActive(false);

        if (Utilities.IsValid(heatArea))
            heatArea.SetActive(false);
        if (Utilities.IsValid(particleField))
            particleField.Stop();
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

    public override void OnDeserialization()
    {
        if (!Utilities.IsValid(particleField)) return;

        if (isActive)
            particleField.Play();
        else
            particleField.Stop();
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
        StopHeat();

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

        InitSelectedWeapon();
        StartHeat();

        RequestSerialization();
    }

    private void InitSelectedWeapon()
    {
        Vector3 pos = Utilities.IsValid(itemSpawnPoint)
            ? itemSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        currentItem.transform.position = pos;
        currentItem.transform.rotation = Quaternion.identity;
        currentItem.SetActive(true);
    }

    private void StartHeat()
    {
        isActive = true;
        if (Utilities.IsValid(heatArea))
            heatArea.SetActive(true);
        if (Utilities.IsValid(particleField))
            particleField.Play();
        RequestSerialization();
    }

    private void StopHeat()
    {
        isActive = false;
        if (Utilities.IsValid(heatArea))
            heatArea.SetActive(false);
        if (Utilities.IsValid(particleField))
            particleField.Stop();
        RequestSerialization();
    }
}
