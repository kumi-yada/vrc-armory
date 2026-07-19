
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class BowVisuals : UdonSharpBehaviour
{
    [SerializeField] private GameObject arrowVisual;
    [SerializeField] private float arrowStartDistance = 0.5f;
    [SerializeField] private float arrowEndDistance = 0.5f;
    [SerializeField] private SkinnedMeshRenderer bowMesh;

    [UdonSynced] private bool active;
    private float pullDistance;

    public bool IsLoaded()
    {
        return active;
    }

    public float GetPullDistance()
    {
        return pullDistance;
    }

    public void SetArrowActive(bool active)
    {
        this.active = active;
        arrowVisual.SetActive(active);
        RequestSerialization();
    }

    public void SetPullDistance(float pullDistance)
    {
        this.pullDistance = pullDistance;
    }

    void Update()
    {
        var dist = Mathf.Lerp(arrowStartDistance, arrowEndDistance, pullDistance);
        arrowVisual.transform.localPosition = new Vector3(0f, dist, 0f);
        bowMesh.SetBlendShapeWeight(0, pullDistance * 100f);
    }

    public override void OnDeserialization()
    {
        arrowVisual.SetActive(active);
    }

}
