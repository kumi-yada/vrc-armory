using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Anvil : UdonSharpBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
    }

    public void ShowUI()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = true;
    }

    public void HideUI()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
    }
}
