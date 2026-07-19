
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class OwnerFollower : UdonSharpBehaviour
{
    [SerializeField] private HumanBodyBones attachBone = HumanBodyBones.Head;

    void Update()
    {
        var owner = Networking.GetOwner(gameObject);
        Vector3 bonePos = owner.GetBonePosition(attachBone);
        Quaternion boneRot = owner.GetBoneRotation(attachBone);
        transform.SetPositionAndRotation(bonePos, boneRot);
    }
}
