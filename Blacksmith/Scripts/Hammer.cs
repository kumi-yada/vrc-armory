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

        if (!smite.IsActive || smite.IsFinished)
        {
            Debug.Log("Inactive or finished smite point");
            return;
        }

        Debug.Log("Hitting with Hammer");
        smite.CheckHit();
    }
}
