using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Arrow : UdonSharpBehaviour
{
    private bool isInEditor;
    private VRCObjectPool pool;

    [UdonSynced] private int bone = -1001;

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        if (value || isInEditor || !Networking.IsOwner(gameObject)) return;
        if (args.handType == HandType.LEFT && bone != (int)HumanBodyBones.LeftHand) return;
        if (args.handType == HandType.RIGHT && bone != (int)HumanBodyBones.RightHand) return;
        ReturnToQuiver();
    }

    void Update()
    {
        if (isInEditor || bone == -1001) return;

        var owner = Networking.GetOwner(gameObject);
        Vector3 bonePos = owner.GetBonePosition((HumanBodyBones)bone);
        Quaternion boneRot = owner.GetBoneRotation((HumanBodyBones)bone);
        transform.SetPositionAndRotation(bonePos, boneRot);
    }

    public void Initialize(HumanBodyBones handBone, VRCObjectPool pool)
    {
        if (!Networking.IsOwner(gameObject)) return;

        bone = (int)handBone;
        this.pool = pool;
        RequestSerialization();
    }

    public void ReturnToQuiver()
    {
        pool.Return(gameObject);
    }
}
