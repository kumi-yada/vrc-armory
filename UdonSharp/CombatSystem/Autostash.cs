using MMMaellon;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class Autostash : UdonSharpBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float checkInterval = 1f;

    [Header("References")]
    [SerializeField] private SmartObjectSync sync;

    private VRCPlayerApi localPlayer;
    private bool isInEditor;
    private float lastCheckTime;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        isInEditor = localPlayer == null;

        if (!Utilities.IsValid(sync))
            sync = GetComponent<SmartObjectSync>();

        lastCheckTime = Time.time - checkInterval;
    }

    void Update()
    {
        if (isInEditor) return;
        if (!Networking.IsOwner(gameObject)) return;
        if (!Utilities.IsValid(sync)) return;
        if (sync.IsHeld()) return;
        if (Time.time - lastCheckTime < checkInterval) return;

        lastCheckTime = Time.time;

        var owner = Networking.GetOwner(gameObject);
        if (!Utilities.IsValid(owner)) return;

        var headData = owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        var distance = Vector3.Distance(transform.position, headData.position);

        if (distance > maxDistance)
        {
            StashToOwner(owner);
        }
    }

    private void StashToOwner(VRCPlayerApi owner)
    {
        var chestPos = owner.GetBonePosition(HumanBodyBones.Chest);
        sync.TeleportTo(chestPos, transform.rotation, Vector3.zero, Vector3.zero);
    }
}
