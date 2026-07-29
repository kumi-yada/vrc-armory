using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Workstation : UdonSharpBehaviour
{
    private const int FREE = -1;

    [SerializeField] private StationPod stationPodReference;

    [UdonSynced] private int occupantPlayerId = FREE;

    void Start()
    {
        occupantPlayerId = FREE;
    }

    public override void Interact()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return;

        if (occupantPlayerId == local.playerId)
        {
            ResetLocalWorkstation(local);
            occupantPlayerId = FREE;
            RequestSerialization();
            return;
        }

        if (occupantPlayerId != FREE) return;

        Networking.SetOwner(local, gameObject);
        occupantPlayerId = local.playerId;
        RequestSerialization();
        MoveLocalWorkstation();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return;
        if (!Networking.IsMaster) return;
        if (occupantPlayerId != player.playerId) return;

        occupantPlayerId = FREE;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        RequestSerialization();
    }

    private void MoveLocalWorkstation()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return;
        if (occupantPlayerId != local.playerId) return;
        if (!Utilities.IsValid(stationPodReference)) return;

        Component found = Networking.FindComponentInPlayerObjects(local, stationPodReference);
        if (!Utilities.IsValid(found)) return;

        StationPod pod = (StationPod)found;
        if (!Utilities.IsValid(pod)) return;

        pod.MoveTo(transform.position, transform.rotation);
    }

    private void ResetLocalWorkstation(VRCPlayerApi local)
    {
        if (!Utilities.IsValid(stationPodReference)) return;

        Component found = Networking.FindComponentInPlayerObjects(local, stationPodReference);
        if (!Utilities.IsValid(found)) return;

        StationPod pod = (StationPod)found;
        if (!Utilities.IsValid(pod)) return;

        pod.Deactivate();
    }
}
