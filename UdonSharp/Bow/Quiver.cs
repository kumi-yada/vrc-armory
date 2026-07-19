using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Quiver : UdonSharpBehaviour
{
    [SerializeField] private VRCObjectPool pool;

    public void OnGrabLeftHand()
    {
        SpawnForHand(HumanBodyBones.LeftHand);
    }

    public void OnGrabRightHand()
    {
        SpawnForHand(HumanBodyBones.RightHand);
    }

    private void SpawnForHand(HumanBodyBones hand)
    {
        if (!Networking.IsOwner(gameObject)) return;

        GameObject spawned = pool.TryToSpawn();
        if (!Utilities.IsValid(spawned)) return;

        if (Networking.LocalPlayer != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, spawned);
        }

        Arrow arrow = spawned.GetComponent<Arrow>();
        if (Utilities.IsValid(arrow))
        {
            arrow.Initialize(hand, pool);
        }
    }

}
