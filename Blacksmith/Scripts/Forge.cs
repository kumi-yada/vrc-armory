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
    [SerializeField] private Transform spawnItemContainer;
    [SerializeField] private Transform itemSpawnPoint;

    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private GameObject recipeListPage;
    [SerializeField] private GameObject recipeDetailsPage;
    [SerializeField] private TextMeshProUGUI recipeNameText;

    [Header("Heat")]
    [SerializeField] private GameObject heatArea;
    [SerializeField] private ParticleSystem particleField;
    [SerializeField] public Slider heatSlider;
    [SerializeField] public Image optimalRangeImage;
    [SerializeField] public RectTransform optimalRangeMarkerLow;
    [SerializeField] public RectTransform optimalRangeMarkerHigh;

    private int selectedRecipeIndex = -1;
    private SmiteWeapon currentItem;
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

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;
        if (!Utilities.IsValid(uiCanvas)) return;

        uiCanvas.enabled = true;
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;
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

        Debug.Log("Forge: SelectRecipe: index = " + index);
        selectedRecipeIndex = index;
        SpawnSmiteWeapon();
        if (currentItem == null) return;

        recipeListPage.SetActive(false);
        recipeDetailsPage.SetActive(true);
        if (Utilities.IsValid(recipeNameText))
            recipeNameText.text = currentItem.recipeName;
            Debug.Log("Forge: SelectRecipe: selected recipe = " + currentItem.recipeName);
    }

    public void ClearCurrentItem()
    {
        if (!Networking.IsOwner(gameObject)) return;

        Debug.Log("Forge: ClearCurrentItem: clearing current item");
        currentItem = null;
        StopHeat();

        recipeDetailsPage.SetActive(false);
        recipeListPage.SetActive(true);
    }

    public void CancelSelection()
    {
        Debug.Log("Forge: CancelSelection: clearing current item");
        if (!Networking.IsOwner(gameObject)) return;

        if (Utilities.IsValid(currentItem))
            currentItem.gameObject.SetActive(false);
        ClearCurrentItem();
    }

    public void SpawnSmiteWeapon()
    {
        if (!Utilities.IsValid(spawnItemContainer) || selectedRecipeIndex < 0 || selectedRecipeIndex >= spawnItemContainer.childCount)
            return;

        if (Utilities.IsValid(currentItem))
            currentItem.gameObject.SetActive(false);

        Transform child = spawnItemContainer.GetChild(selectedRecipeIndex);
        GameObject go = child.gameObject;
        currentItem = go.GetComponent<SmiteWeapon>();
        if (!Utilities.IsValid(currentItem))
        {
            Debug.Log("Forge: SpawnSmiteWeapon: currentItem is null for index " + selectedRecipeIndex);
            return;
        }

        InitSelectedWeapon();
        currentItem.spawnItemIndex = selectedRecipeIndex;
        currentItem.forge = this;

        StartHeat();
        RequestSerialization();
    }

    private void InitSelectedWeapon()
    {
        Vector3 pos = Utilities.IsValid(itemSpawnPoint)
            ? itemSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        currentItem.ResetState();
        currentItem.transform.position = pos;
        currentItem.transform.rotation = Quaternion.identity;
        currentItem.gameObject.SetActive(true);
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

    public int ItemCount
    {
        get
        {
            if (!Utilities.IsValid(spawnItemContainer)) return 0;
            return spawnItemContainer.childCount;
        }
    }

    public SmiteWeapon GetItemByIndex(int index)
    {
        if (!Utilities.IsValid(spawnItemContainer) || index < 0 || index >= spawnItemContainer.childCount)
            return null;
        Transform child = spawnItemContainer.GetChild(index);
        return child.GetComponent<SmiteWeapon>();
    }

    // Recipe selection shortcuts
    public void SelectKnife() { SelectRecipe(0); }
}
