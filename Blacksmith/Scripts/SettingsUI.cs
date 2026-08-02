using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SettingsUI : UdonSharpBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle particlesToggle;

    void Start()
    {
        Settings settings = GetSettings();
        if (settings == null) return;

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(settings.GetBgmVolume());
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(settings.GetSfxVolume());
        if (particlesToggle != null)
            particlesToggle.SetIsOnWithoutNotify(settings.GetParticlesEnabled());
    }

    public void OnBgmSliderChanged()
    {
        if (bgmSlider == null) return;
        Settings settings = GetSettings();
        if (settings == null) return;
        settings.SetBgmVolume(bgmSlider.value);
    }

    public void OnSfxSliderChanged()
    {
        if (sfxSlider == null) return;
        Settings settings = GetSettings();
        if (settings == null) return;
        settings.SetSfxVolume(sfxSlider.value);
    }

    public void OnParticlesToggled()
    {
        if (particlesToggle == null) return;
        Settings settings = GetSettings();
        if (settings == null) return;
        settings.SetParticlesEnabled(particlesToggle.isOn);
    }

    private Settings GetSettings()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return null;

        GameObject[] playerObjects = Networking.GetPlayerObjects(local);
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (!Utilities.IsValid(playerObjects[i])) continue;
            Settings settings = playerObjects[i].GetComponentInChildren<Settings>();
            if (Utilities.IsValid(settings)) return settings;
        }

        return null;
    }
}
