using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PlayerMenu : UdonSharpBehaviour
{
    public Canvas canvas;
    public float distance = 0.8f;
    public float heightOffset = -0.15f;
    public string vrAxisName = "Oculus_CrossPlatform_SecondaryThumbstickVertical";
    public float vrHoldDuration = 2f;
    public float vrStickThreshold = 0.5f;

    private VRCPlayerApi localPlayer;
    private bool isInEditor;
    private bool isOpen;
    private float vrHoldTime;
    private bool vrStickWasNeutral;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        isInEditor = localPlayer == null;
        canvas.enabled = false;
        vrStickWasNeutral = true;
    }

    void Update()
    {
        if (isInEditor) return;

        if (localPlayer.IsUserInVR())
        {
            HandleVRInput();
        }
        else
        {
            HandleDesktopInput();
        }
    }

    private void HandleDesktopInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Toggle();
        }
    }

    private void HandleVRInput()
    {
        float stickY = Input.GetAxis(vrAxisName);
        bool holdingDown = stickY < -vrStickThreshold;

        if (holdingDown)
        {
            if (vrStickWasNeutral)
            {
                vrHoldTime += Time.deltaTime;
                if (vrHoldTime >= vrHoldDuration)
                {
                    Toggle();
                    vrHoldTime = 0f;
                    vrStickWasNeutral = false;
                }
            }
        }
        else
        {
            vrHoldTime = 0f;
            vrStickWasNeutral = true;
        }
    }

    private void Toggle()
    {
        isOpen = !isOpen;
        canvas.enabled = isOpen;

        if (isOpen)
        {
            var head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Vector3 targetPos = head.position + head.rotation * new Vector3(0f, heightOffset, distance);
            transform.SetPositionAndRotation(targetPos, head.rotation);
        }
    }
}
