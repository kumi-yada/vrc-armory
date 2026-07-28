using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    private Weapon containedWeapon;

    public void OnTriggerEnter(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (!Utilities.IsValid(weapon))
            return;

        containedWeapon = weapon;
    }

    public void OnTriggerExit(Collider other)
    {
        Weapon weapon = other.GetComponentInParent<Weapon>();
        if (!Utilities.IsValid(weapon))
            return;

        if (containedWeapon == weapon)
            containedWeapon = null;
    }

    private void Update()
    {
        if (!Utilities.IsValid(containedWeapon))
            return;

        float heat = containedWeapon.GetHeat();
        if (heat <= 0f)
            return;

        heat -= coolRate * Time.deltaTime;
        if (heat <= 0f)
        {
            heat = 0f;
            containedWeapon.isHeated = false;
            containedWeapon.EvaluateQuality();
        }

        containedWeapon.SetHeat(heat);
    }
}
