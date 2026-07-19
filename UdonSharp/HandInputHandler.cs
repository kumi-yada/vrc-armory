
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class HandInputHandler : UdonSharpBehaviour
{
    public UdonBehaviour triggerObject;
    public string leftHandGrabMethod = "OnGrabLeftHand";
    public string rightHandGrabMethod = "OnGrabRightHand";
    public string leftHandReleaseMethod = "OnReleaseLeftHand";
    public string rightHandReleaseMethod = "OnReleaseRightHand";
    private bool isLeftHandTriggered = false;
    private bool isRightHandTriggered = false;

    public bool IsHandInside()
    {
        return isLeftHandTriggered || isRightHandTriggered;
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        if (!Networking.IsOwner(gameObject) || !Networking.LocalPlayer.IsUserInVR()) return;

        var bone = args.handType == HandType.LEFT ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
        if (bone == HumanBodyBones.LeftHand && !isLeftHandTriggered) return;
        if (bone == HumanBodyBones.RightHand && !isRightHandTriggered) return;

        Debug.Log($"InputGrab: {bone} - {value}");
        if (value)
        {
            SendGrabEvent(bone);
        }
        else
        {
            SendReleaseEvent(bone);
        }
    }

    private void SendReleaseEvent(HumanBodyBones bone)
    {
        string customMethodName = bone == HumanBodyBones.LeftHand ? leftHandReleaseMethod : rightHandReleaseMethod;
        triggerObject.SendCustomEvent(customMethodName);
    }

    private void SendGrabEvent(HumanBodyBones bone)
    {
        string customMethodName = bone == HumanBodyBones.LeftHand ? leftHandGrabMethod : rightHandGrabMethod;
        triggerObject.SendCustomEvent(customMethodName);
    }

    public void OnTriggerEnter(Collider other)
    {
        string n = other.name.ToLower();
        if (n.Contains("lefthand"))
        {
            isLeftHandTriggered = true;
            Debug.Log("Left hand triggered");
            return;
        }

        if (n.Contains("righthand"))
        {
            isRightHandTriggered = true;
            Debug.Log("Right hand triggered");
            return;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        string n = other.name.ToLower();
        if (n.Contains("lefthand") && isLeftHandTriggered)
        {
            isLeftHandTriggered = false;
        }
        if (n.Contains("righthand") && isRightHandTriggered)
        {
            isRightHandTriggered = false;
        }
    }
}
