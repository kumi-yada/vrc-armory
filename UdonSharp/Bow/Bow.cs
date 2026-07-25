using System;
using MMMaellon;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Bow : SmartObjectSyncListener
{
    [SerializeField] private VRBowGrip vrBowGrip;
    [SerializeField] private DesktopBowGrip desktopGrip;
    [SerializeField] private VRCObjectPool arrowPool;
    [SerializeField] private BowVisuals bowVisuals;
    [SerializeField] private GameObject quiver;
    [SerializeField] private SmartObjectSync sync;
    [SerializeField] private WeaponAutoScale weaponScale;
    [SerializeField] private float minForceToShoot = 0.2f;
    [SerializeField] private float arrowSpeed = 30f;

    private VRCPickup pickup;
    private int shotCounter;

    void Start()
    {
        pickup = GetComponent<VRCPickup>();
        bowVisuals.SetArrowActive(false);
        bowVisuals.SetPullDistance(0.0f);
        pickup.pickupable = Networking.IsOwner(gameObject);

        var owner = Networking.GetOwner(gameObject);
        if (owner.IsUserInVR())
        {
            pickup.AutoHold = VRC_Pickup.AutoHoldMode.No;
            pickup.orientation = VRC_Pickup.PickupOrientation.Any;
            vrBowGrip.gameObject.SetActive(true);
            desktopGrip.gameObject.SetActive(false);
        }
        else
        {
            pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            pickup.orientation = VRC_Pickup.PickupOrientation.Grip;
            vrBowGrip.gameObject.SetActive(false);
            desktopGrip.gameObject.SetActive(true);
        }
    }

    public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
    {
        if (s != sync) return;

        if (newState >= SmartObjectSync.STATE_CUSTOM)
        {
            SetLoaded(false);
        }
    }

    public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
    {
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        quiver.SetActive(active);

        var owner = Networking.GetOwner(gameObject);
        vrBowGrip.gameObject.SetActive(owner.IsUserInVR() && active);
        desktopGrip.gameObject.SetActive(!owner.IsUserInVR() && active);

        if (Networking.IsOwner(gameObject))
        {
            StashBow();
        }
    }

    public bool IsHeld()
    {
        return sync.IsHeld();
    }

    public void SetLoaded(bool loaded)
    {
        if (!Networking.IsOwner(gameObject)) return;
        bowVisuals.SetArrowActive(loaded);
    }

    public void SetPullDistance(float pull)
    {
        bowVisuals.SetPullDistance(pull);
    }

    void Update()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (!Networking.LocalPlayer.IsUserInVR() && Input.GetKeyDown(KeyCode.T))
        {
            if (sync.state >= SmartObjectSync.STATE_CUSTOM)
            {
                UnstashBow();
            }
            else
            {
                StashBow();
            }
        }
    }

    private void UnstashBow()
    {
        sync.TeleportTo(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
        SendCustomEventDelayedFrames(nameof(PositionInFront), 1);
    }

    public void PositionInFront()
    {
        var owner = Networking.GetOwner(gameObject);
        var chestPos = owner.GetBonePosition(HumanBodyBones.Chest);
        var chestRot = owner.GetBoneRotation(HumanBodyBones.Chest);
        var pos = chestPos + chestRot * Vector3.forward * GetAvatarScale();
        sync.TeleportTo(pos, chestRot, Vector3.zero, Vector3.zero);
    }

    private float GetAvatarScale()
    {
        if (!Utilities.IsValid(weaponScale)) weaponScale = GetComponent<WeaponAutoScale>();
        var owner = Networking.GetOwner(gameObject);
        return Utilities.IsValid(weaponScale) ? weaponScale.GetScaleForOwner(owner) : 1f;
    }

    private void StashBow()
    {
        SetLoaded(false);
        var owner = Networking.GetOwner(gameObject);
        var pos = owner.GetBonePosition(HumanBodyBones.Chest);
        sync.TeleportTo(pos, transform.rotation, Vector3.zero, Vector3.zero);
    }

    public override void OnPickup()
    {
        sync.DisablePickupable();
    }

    public override void OnDrop()
    {
        sync.EnablePickupable();
        desktopGrip.StopCharging();
        vrBowGrip.Release();
    }

    public override void OnPickupUseDown()
    {
        if (!Networking.IsOwner(gameObject) || Networking.LocalPlayer.IsUserInVR()) return;
        SetLoaded(true);
        desktopGrip.StartCharging();
    }

    public override void OnPickupUseUp()
    {
        if (!Networking.IsOwner(gameObject) || Networking.LocalPlayer.IsUserInVR()) return;
        desktopGrip.StopCharging();
        ShootArrow();
    }

    private FlyingArrow FindStuckArrow()
    {
        FlyingArrow oldest = null;
        int oldestOrder = int.MaxValue;
        for (int i = 0; i < arrowPool.transform.childCount; i++)
        {
            var child = arrowPool.transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;
            var arrow = child.GetComponent<FlyingArrow>();
            if (arrow != null && arrow.IsStuck && arrow.GetShotOrder() < oldestOrder)
            {
                oldest = arrow;
                oldestOrder = arrow.GetShotOrder();
            }
        }
        return oldest;
    }

    public void ShootArrow()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (!bowVisuals.IsLoaded()) return;

        var force = bowVisuals.GetPullDistance();
        if (force < minForceToShoot) return;

        GameObject spawned = arrowPool.TryToSpawn();
        if (!Utilities.IsValid(spawned))
        {
            var stuck = FindStuckArrow();
            if (stuck != null)
            {
                stuck.ReturnToPool();
                spawned = arrowPool.TryToSpawn();
                if (!Utilities.IsValid(spawned)) return;
            }
            else return;
        }

        var appliedForce = Mathf.Lerp(0f, arrowSpeed, force);
        var shootDir = vrBowGrip.gameObject.activeSelf ? (transform.position - vrBowGrip.transform.position).normalized : transform.forward;
        var arrow = spawned.GetComponent<FlyingArrow>();
        arrow.SetShotOrder(shotCounter++);
        arrow.ApplyForce(transform.position, shootDir, appliedForce, arrowPool);
        SetLoaded(false);
    }

}
