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
    [SerializeField] private RectTransform sliderValueMarker;

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
        if (Utilities.IsValid(uiCanvas) && uiCanvas.enabled)
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (Utilities.IsValid(player))
            {
                Vector3 target = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                uiCanvas.transform.rotation = Quaternion.LookRotation(target - uiCanvas.transform.position);
            }
        }

        if (!Utilities.IsValid(activeSmitePoint)) return;
        if (!activeSmitePoint.IsActive || activeSmitePoint.IsFinished) return;

        float normValue = activeSmitePoint.SmiteValue / 100f;
        float hitFrom = activeSmitePoint.hitAreaFrom / 100f;
        float hitTo = activeSmitePoint.hitAreaTo / 100f;

        bool leftToRight = smiteSlider.direction == Slider.Direction.LeftToRight;
        bool bottomToTop = smiteSlider.direction == Slider.Direction.BottomToTop;

        if (Utilities.IsValid(hitZoneImage))
        {
            if (leftToRight)
            {
                hitZoneImage.rectTransform.anchorMin = new Vector2(hitFrom, 0f);
                hitZoneImage.rectTransform.anchorMax = new Vector2(hitTo, 1f);
                hitZoneImage.rectTransform.anchoredPosition = Vector2.zero;
            }
            else if (bottomToTop)
            {
                hitZoneImage.rectTransform.anchorMin = new Vector2(0f, hitFrom);
                hitZoneImage.rectTransform.anchorMax = new Vector2(1f, hitTo);
                hitZoneImage.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        if (Utilities.IsValid(hitZoneCenterMarker))
        {
            float center = (hitFrom + hitTo) / 2f;
            if (leftToRight)
            {
                hitZoneCenterMarker.anchorMin = new Vector2(center, 0f);
                hitZoneCenterMarker.anchorMax = new Vector2(center, 1f);
                hitZoneCenterMarker.anchoredPosition = Vector2.zero;
            }
            else if (bottomToTop)
            {
                hitZoneCenterMarker.anchorMin = new Vector2(0f, center);
                hitZoneCenterMarker.anchorMax = new Vector2(1f, center);
                hitZoneCenterMarker.anchoredPosition = Vector2.zero;
            }
        }

        if (Utilities.IsValid(sliderValueMarker))
        {
            if (leftToRight)
            {
                sliderValueMarker.anchorMin = new Vector2(normValue, 0f);
                sliderValueMarker.anchorMax = new Vector2(normValue, 1f);
                sliderValueMarker.anchoredPosition = Vector2.zero;
            }
            else if (bottomToTop)
            {
                sliderValueMarker.anchorMin = new Vector2(0f, normValue);
                sliderValueMarker.anchorMax = new Vector2(1f, normValue);
                sliderValueMarker.anchoredPosition = Vector2.zero;
            }
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
