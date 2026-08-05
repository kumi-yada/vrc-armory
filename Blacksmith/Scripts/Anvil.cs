using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Anvil : UdonSharpBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider smiteSlider;
    [SerializeField] private Image hitZoneImage;
    [SerializeField] private RectTransform hitZoneCenterMarker;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private RectTransform sliderValueMarker;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private AudioSource hitAudio;

    [System.NonSerialized] public SmitePoint activeSmitePoint;

    private float flashTimer;
    private Color flashColor;
    private Color originalFillColor;

    private float resultDisplayTimer;
    private string resultLabelText;

    void Start()
    {
        if (Utilities.IsValid(uiCanvas))
            uiCanvas.enabled = false;
        if (Utilities.IsValid(sliderFillImage))
            originalFillColor = sliderFillImage.color;
        if (Utilities.IsValid(statusText))
            statusText.gameObject.SetActive(false);
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
        if (Utilities.IsValid(statusText))
            statusText.gameObject.SetActive(false);
    }

    public void OnSmiteResult(bool hit)
    {
        flashTimer = 0.15f;
        flashColor = hit ? Color.green : Color.red;
    }

    private string GetAccuracyLabel(float accuracy)
    {
        if (accuracy >= 0.9f) return "Perfect";
        if (accuracy >= 0.7f) return "Excellent";
        if (accuracy >= 0.5f) return "Good";
        if (accuracy >= 0.2f) return "Fair";
        if (accuracy > 0f) return "Poor";
        return "Miss";
    }

    [NetworkCallable]
    public void PlaySmiteEffects(float accuracy)
    {
        resultDisplayTimer = 1.5f;
        resultLabelText = GetAccuracyLabel(accuracy);

        if (accuracy >= 0.6f)
        {
            if (Utilities.IsValid(hitParticles))
            {
                if (Utilities.IsValid(activeSmitePoint))
                    hitParticles.transform.position = activeSmitePoint.transform.position;
                hitParticles.Play();
            }
        }

        if (Utilities.IsValid(hitAudio))
        {
            hitAudio.pitch = Mathf.Lerp(0.6f, 1.2f, accuracy);
            hitAudio.Play();
        }
    }

    void Update()
    {
        if (Utilities.IsValid(uiCanvas) && uiCanvas.enabled)
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            if (Utilities.IsValid(player))
            {
                Vector3 target = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                uiCanvas.transform.rotation = Quaternion.LookRotation(target - uiCanvas.transform.position) * Quaternion.Euler(0f, 180f, 0f);
            }
        }

        if (!Utilities.IsValid(activeSmitePoint)) return;

        if (Utilities.IsValid(statusText))
        {
            if (resultDisplayTimer > 0f)
            {
                resultDisplayTimer -= Time.deltaTime;
                statusText.gameObject.SetActive(true);
                statusText.text = resultLabelText;
            }
            else
            {
                bool weaponValid = Utilities.IsValid(activeSmitePoint.weapon);
                bool isOptimal = weaponValid && activeSmitePoint.weapon.IsHeatOptimal();
                if (isOptimal)
                {
                    statusText.gameObject.SetActive(false);
                }
                else if (weaponValid)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = activeSmitePoint.weapon.IsHeatTooHot() ? "Too Hot" : "Too Cold";
                }
                else
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = "";
                }
            }
        }

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
