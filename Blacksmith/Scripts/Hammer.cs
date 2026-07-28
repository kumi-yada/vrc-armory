using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class Hammer : UdonSharpBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SmitePoint smite = other.GetComponentInParent<SmitePoint>();
        if (!Utilities.IsValid(smite)) return;
        if (!smite.IsActive || smite.IsFinished) return;

        smite.CheckHit();
    }
}
