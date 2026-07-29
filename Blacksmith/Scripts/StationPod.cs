using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class StationPod : UdonSharpBehaviour
{
    [UdonSynced] private Vector3 syncedPosition;
    [UdonSynced] private Quaternion syncedRotation;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        syncedPosition = originalPosition;
        syncedRotation = originalRotation;
    }

    public void ResetToOriginal()
    {
        if (!Networking.IsOwner(gameObject)) return;

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        RequestSerialization();
    }

    public void Deactivate()
    {
        if (!Networking.IsOwner(gameObject)) return;

        transform.SetPositionAndRotation(originalPosition, originalRotation);
        RequestSerialization();
    }

    public void MoveTo(Vector3 pos, Quaternion rot)
    {
        if (!Networking.IsOwner(gameObject)) return;

        transform.SetPositionAndRotation(pos, rot);
        RequestSerialization();
    }

    public override void OnPreSerialization()
    {
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
    }

    public override void OnDeserialization()
    {
        transform.SetPositionAndRotation(syncedPosition, syncedRotation);
        bool atOriginal = syncedPosition == originalPosition && syncedRotation == originalRotation;
        gameObject.SetActive(!atOriginal);
    }
}
