using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class DamageLabel : UdonSharpBehaviour
{
    [SerializeField] private float floatHeight = 1.5f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float startFadeAt = 0.5f;
    [SerializeField] private bool billboard = true;
    [SerializeField] private bool constantSize = true;
    [SerializeField] private float scaleMultiplier = 1f;

    private Vector3 startPosition;
    private float elapsed;
    private bool active;

    private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (!Utilities.IsValid(textMesh))
            textMesh = GetComponentInChildren<TextMeshProUGUI>();

        gameObject.SetActive(false);
    }

    public void Show(float damage, Vector3 position)
    {
        startPosition = position;
        elapsed = 0f;
        active = true;

        if (Utilities.IsValid(textMesh))
        {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
            Color c = textMesh.color;
            c.a = 1f;
            textMesh.color = c;
        }

        transform.position = position;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!active) return;

        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        transform.position = startPosition + Vector3.up * (floatHeight * t);

        if (billboard || constantSize)
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(localPlayer))
            {
                var head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                if (billboard)
                {
                    transform.rotation = Quaternion.LookRotation(transform.position - head.position);
                }
                if (constantSize)
                {
                    float distance = Vector3.Distance(transform.position, head.position);
                    transform.localScale = Vector3.one * distance * scaleMultiplier;
                }
            }
        }

        if (Utilities.IsValid(textMesh) && t > startFadeAt)
        {
            float fadeT = Mathf.Clamp01((t - startFadeAt) / (1f - startFadeAt));
            Color c = textMesh.color;
            c.a = 1f - fadeT;
            textMesh.color = c;
        }

        if (elapsed >= lifetime)
        {
            active = false;
            gameObject.SetActive(false);
        }
    }
}
