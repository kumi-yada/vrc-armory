using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class DamageLabelPool : UdonSharpBehaviour
{
    [SerializeField] private DamageLabel[] labels;

    private int nextIndex;

    public void Show(float damage, Vector3 position)
    {
        if (labels == null || labels.Length == 0) return;

        var label = labels[nextIndex];
        nextIndex = (nextIndex + 1) % labels.Length;

        if (Utilities.IsValid(label))
            label.Show(damage, position);
    }
}
