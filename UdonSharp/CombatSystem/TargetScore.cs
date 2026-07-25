
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TargetScore : UdonSharpBehaviour
{
    [Header("References")]
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Transform centerPoint;

    [Header("Score Settings")]
    [SerializeField] private float maxDistance = 1f;
    [SerializeField] private float maxScore = 100f;
    [SerializeField] private float minScore = 10f;

    [Header("Events")]
    [SerializeField] private UdonBehaviour onScoreEvent;
    [SerializeField] private string onScoreEventName = "OnScore";
    [SerializeField] private string scoreVariableName = "score";

    private int lastScore;

    void Start()
    {
        if (hurtbox != null)
        {
            hurtbox.onHitEvent = (UdonBehaviour)(object)this;
            hurtbox.onHitEventName = nameof(OnHit);
        }
    }

    public void OnHit()
    {
        if (hurtbox == null) return;

        Vector3 hitPoint = hurtbox.GetLastHitPoint();
        Vector3 center = centerPoint != null ? centerPoint.position : transform.position;

        float distance = Vector3.Distance(hitPoint, center);
        float t = Mathf.Clamp01(distance / maxDistance);
        float score = Mathf.Lerp(maxScore, minScore, t);

        lastScore = Mathf.RoundToInt(score);

        if (onScoreEvent != null)
        {
            onScoreEvent.SetProgramVariable(scoreVariableName, lastScore);
            onScoreEvent.SendCustomEvent(onScoreEventName);
            Debug.Log($"TargetScore: Hit registered. Score: {lastScore}");
        }
    }

    public int GetLastScore() { return lastScore; }
}