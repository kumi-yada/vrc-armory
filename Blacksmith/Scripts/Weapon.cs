using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Weapon : UdonSharpBehaviour
{
    [System.NonSerialized] public RecipeData recipe;
    [System.NonSerialized] public float currentHeat;
    [System.NonSerialized] public bool isHeated;
    [UdonSynced] public bool isHeld;

    bool isInEditor;

    void Start()
    {
        isInEditor = Networking.LocalPlayer == null;
    }

    void Update()
    {
        if (isInEditor || !isHeld)
            return;

        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        if (!Utilities.IsValid(owner))
            return;

        Tongs tongs = FindTongs(owner);
        if (!Utilities.IsValid(tongs) || tongs.attachPoint == null)
            return;

        transform.SetPositionAndRotation(tongs.attachPoint.position, tongs.attachPoint.rotation);
    }

    private Tongs FindTongs(VRCPlayerApi player)
    {
        var objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            Tongs tongs = objects[i].GetComponentInChildren<Tongs>();
            if (Utilities.IsValid(tongs)) return tongs;
        }
        return null;
    }

    public void Setup(RecipeData recipeData, float heat)
    {
        recipe = recipeData;
        currentHeat = heat;
        isHeated = true;
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public float GetHeat()
    {
        return currentHeat;
    }

    public void SetHeat(float heat)
    {
        currentHeat = heat;
    }

    public void OnGrabbed()
    {
        isHeld = true;
        RequestSerialization();
    }

    public void OnReleased()
    {
        isHeld = false;
        RequestSerialization();
    }
}
