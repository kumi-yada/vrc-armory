
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InventoryArea : UdonSharpBehaviour
{
    [Header("Settings")]
    [SerializeField] private string mode = Storage.MODE_STASH;

    [Header("References")]
    [SerializeField] private Transform displayTransform;

    private Storage storage;

    void Start()
    {
        storage = Find(Networking.LocalPlayer);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;

        if (!Utilities.IsValid(storage)){
            Debug.Log($"[InventoryArea] Player {player.displayName} entered inventory area, but no storage was found");
            return;
        }

        storage.SetMode(mode);
        storage.gameObject.SetActive(true);
        Debug.Log($"[InventoryArea] Player {player.displayName} entered inventory area, showing storage {storage.name} in mode {mode}");

        if (Utilities.IsValid(displayTransform))
            storage.transform.SetPositionAndRotation(displayTransform.position, displayTransform.rotation);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!player.isLocal) return;

        if (Utilities.IsValid(storage))
            storage.transform.SetPositionAndRotation(new Vector3(0, -100f, 0), Quaternion.identity);
    }

    public Storage Find(VRCPlayerApi player)
    {
        var objects = Networking.GetPlayerObjects(player);
        for (int i = 0; i < objects.Length; i++)
        {
            if (!Utilities.IsValid(objects[i])) continue;
            Storage foundScript = objects[i].GetComponentInChildren<Storage>();
            if (Utilities.IsValid(foundScript)) return foundScript;
        }
        return null;
    }

}
