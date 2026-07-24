
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum Weapon
{
    NONE,
    BOW,
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerWeapon : UdonSharpBehaviour
{
    [Header("Bow")]
    public Bow bow;
    public VRBowGrip vrBowGrip;

    [UdonSynced] private Weapon currentWeapon = Weapon.NONE;

    void Start()
    {
        UpdateWeapon();
    }

    private void UpdateWeapon()
    {
        bow.SetActive(currentWeapon == Weapon.BOW);
    }

    public void SetWeapon()
    {
        currentWeapon = Weapon.BOW;
        UpdateWeapon();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        UpdateWeapon();
    }
}
