
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSharp.Examples.Utilities
{
    /// <summary>
    /// Follows a chosen bone on humanoid avatars using the playerapi
    /// </summary>
    [AddComponentMenu("Udon Sharp/Utilities/Bone Follower")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class BoneFollower : UdonSharpBehaviour 
    {
        public HumanBodyBones trackedBone;
        public Vector3 positionOffset;

        VRCPlayerApi playerApi;
        bool isInEditor;

        void Start()
        {
            playerApi = Networking.LocalPlayer;
            isInEditor = playerApi == null;
        }

        void Update()
        {
            if (isInEditor)
                return;

            Vector3 pos = playerApi.GetBonePosition(trackedBone);
            Quaternion rot = playerApi.GetBoneRotation(trackedBone);

            if (positionOffset != Vector3.zero)
                pos += rot * positionOffset;

            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
