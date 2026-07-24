
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VRBowGrip : UdonSharpBehaviour
{

    [SerializeField] private Bow bow;
    [SerializeField] private GameObject followTarget;
    [SerializeField] private WeaponAutoScale weaponScale;
    [SerializeField] private float maxPullDistance = 1.0f;
    [SerializeField] private float handYOffset = 0.1f;
    [SerializeField] private AimConstraint aimConstraint;

    [UdonSynced] private int pullingBone = -1001;

    public bool IsPulling()
    {
        return pullingBone != -1001;
    }

    public override void OnDeserialization()
    {
        aimConstraint.constraintActive = IsPulling();
    }

    public void Update()
    {
        if (IsPulling())
        {
            var owner = Networking.GetOwner(gameObject);
            var bonePos = owner.GetBonePosition((HumanBodyBones)pullingBone);
            var boneRot = owner.GetBoneRotation((HumanBodyBones)pullingBone);
            var avatarScale = GetAvatarScale();
            transform.position = bonePos + boneRot * Vector3.up * handYOffset * avatarScale;

            var distance = Vector3.Distance(transform.position, followTarget.transform.position);
            bow.SetPullDistance(distance / (maxPullDistance * avatarScale));
        }
        else
        {
            transform.position = followTarget.transform.position;
            bow.SetPullDistance(0.0f);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!other.name.ToLower().Contains("quiverarrow")) return;
        bow.SetLoaded(true);
        other.GetComponent<Arrow>().ReturnToQuiver();
    }

    public void OnGrabLeftHand()
    {
        StartGrab(HumanBodyBones.LeftHand);
    }

    public void OnGrabRightHand()
    {
        StartGrab(HumanBodyBones.RightHand);
    }

    private void StartGrab(HumanBodyBones bone)
    {
        if (IsPulling() || !bow.IsHeld()) return;
        if (!Networking.IsOwner(gameObject)) return;
        pullingBone = (int)bone;
        aimConstraint.constraintActive = true;
        RequestSerialization();
    }

    public void OnReleaseLeftHand()
    {
        if (IsPulling() && (HumanBodyBones)pullingBone == HumanBodyBones.LeftHand)
        {
            ShootBow();
        }
    }

    public void OnReleaseRightHand()
    {
        if (IsPulling() && (HumanBodyBones)pullingBone == HumanBodyBones.RightHand)
        {
            ShootBow();
        }
    }

    private void ShootBow()
    {
        bow.ShootArrow();
        Release();
    }

    public void Release()
    {
        if (!Networking.IsOwner(gameObject)) return;
        pullingBone = -1001;
        aimConstraint.constraintActive = false;
        RequestSerialization();
    }

    private float GetAvatarScale()
    {
        if (!Utilities.IsValid(weaponScale)) weaponScale = GetComponent<WeaponAutoScale>();
        return Utilities.IsValid(weaponScale) ? weaponScale.GetCurrentScale() : 1f;
    }
}
