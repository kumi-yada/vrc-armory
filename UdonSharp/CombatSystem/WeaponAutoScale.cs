using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class WeaponAutoScale : UdonSharpBehaviour
{
    public const float DefaultReferenceAvatarEyeHeight = 1.6f;

    [SerializeField] private float referenceAvatarEyeHeight = DefaultReferenceAvatarEyeHeight;

    private Vector3 initialScale;
    private float currentScale = 1f;
    private bool hasInitialScale;

    void Start()
    {
        EnsureInitialScale();
        ApplyForOwner(Networking.GetOwner(gameObject));
    }

    public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
    {
        var owner = Networking.GetOwner(gameObject);
        if (!Utilities.IsValid(player) || !Utilities.IsValid(owner)) return;
        if (player.playerId != owner.playerId) return;

        ApplyForOwner(owner);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        ApplyForOwner(player);
    }

    public float GetCurrentScale()
    {
        return currentScale;
    }

    public float GetScaleForOwner(VRCPlayerApi owner)
    {
        if (!Utilities.IsValid(owner)) return 1f;
        if (referenceAvatarEyeHeight <= 0f) referenceAvatarEyeHeight = DefaultReferenceAvatarEyeHeight;

        var scale = owner.GetAvatarEyeHeightAsMeters() / referenceAvatarEyeHeight;
        return scale > 0f ? scale : 1f;
    }

    public void ApplyForOwner(VRCPlayerApi owner)
    {
        ApplyScale(GetScaleForOwner(owner));
    }

    public void ApplyScale(float scale)
    {
        EnsureInitialScale();

        currentScale = scale > 0f ? scale : 1f;
        transform.localScale = initialScale * currentScale;
    }

    private void EnsureInitialScale()
    {
        if (hasInitialScale) return;

        initialScale = transform.localScale;
        hasInitialScale = true;
    }
}
