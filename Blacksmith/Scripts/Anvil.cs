using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Anvil : UdonSharpBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider smiteSlider;
    [SerializeField] private Image hitZoneImage;
    [SerializeField] private RectTransform hitZoneCenterMarker;
    [SerializeField] private Image sliderFillImage;

    [System.NonSerialized] public SmitePoint activeSmitePoint;

    private float flashTimer;
    private Color flashColor;
    private Color originalFillColor;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
        if (Utilities.IsValid(sliderFillImage))
            originalFillColor = sliderFillImage.color;
    }

    public void SetActiveSmitePoint(SmitePoint point)
    {
        activeSmitePoint = point;
    }

    public void ShowUI()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = true;
    }

    public void HideUI()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
    }

    public void OnSmiteResult(bool hit)
    {
        flashTimer = 0.15f;
        flashColor = hit ? Color.green : Color.red;
    }

    void Update()
    {
        if (!Utilities.IsValid(activeSmitePoint)) return;
        if (!activeSmitePoint.IsActive || activeSmitePoint.IsFinished) return;

        float normValue = activeSmitePoint.SmiteValue / 100f;
        float hitFrom = activeSmitePoint.hitAreaFrom / 100f;
        float hitTo = activeSmitePoint.hitAreaTo / 100f;

        if (Utilities.IsValid(smiteSlider))
            smiteSlider.value = normValue;

        if (Utilities.IsValid(hitZoneImage))
        {
            hitZoneImage.rectTransform.anchorMin = new Vector2(hitFrom, 0f);
            hitZoneImage.rectTransform.anchorMax = new Vector2(hitTo, 1f);
        }

        if (Utilities.IsValid(hitZoneCenterMarker))
        {
            float center = (hitFrom + hitTo) / 2f;
            hitZoneCenterMarker.anchorMin = new Vector2(center, 0f);
            hitZoneCenterMarker.anchorMax = new Vector2(center, 1f);
        }

        if (Utilities.IsValid(sliderFillImage))
        {
            if (flashTimer > 0f)
            {
                sliderFillImage.color = flashColor;
                flashTimer -= Time.deltaTime;
            }
            else
            {
                sliderFillImage.color = originalFillColor;
            }
        }
    }
}
