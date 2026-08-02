using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Settings : UdonSharpBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource[] sfxSources;

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] particleSystems;

    public const string BGM_VOLUME_KEY = "settings_bgm_volume";
    public const string SFX_VOLUME_KEY = "settings_sfx_volume";
    public const string PARTICLES_KEY = "settings_particles";

    private float bgmBaseVolume = 1f;
    private float[] sfxBaseVolumes;
    private bool[] particlesWerePlaying;

    void Start()
    {
        if (Networking.LocalPlayer == null) return;

        if (bgmSource != null)
            bgmBaseVolume = bgmSource.volume;

        if (sfxSources != null)
        {
            sfxBaseVolumes = new float[sfxSources.Length];
            for (int i = 0; i < sfxSources.Length; i++)
            {
                if (sfxSources[i] != null)
                    sfxBaseVolumes[i] = sfxSources[i].volume;
            }
        }

        if (particleSystems != null)
        {
            particlesWerePlaying = new bool[particleSystems.Length];
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i] != null)
                    particlesWerePlaying[i] = particleSystems[i].isPlaying;
            }
        }

        Load();
    }

    private void Load()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return;

        float bgm = PlayerData.HasKey(local, BGM_VOLUME_KEY) ? PlayerData.GetFloat(local, BGM_VOLUME_KEY) : 1f;
        float sfx = PlayerData.HasKey(local, SFX_VOLUME_KEY) ? PlayerData.GetFloat(local, SFX_VOLUME_KEY) : 1f;
        bool particles = PlayerData.HasKey(local, PARTICLES_KEY) ? PlayerData.GetBool(local, PARTICLES_KEY) : true;

        SetBgmVolume(bgm);
        SetSfxVolume(sfx);
        SetParticlesEnabled(particles);
    }

    public void SetBgmVolume(float multiplier)
    {
        multiplier = Mathf.Clamp01(multiplier);
        if (bgmSource != null)
            bgmSource.volume = bgmBaseVolume * multiplier;
        PlayerData.SetFloat(BGM_VOLUME_KEY, multiplier);
    }

    public void SetSfxVolume(float multiplier)
    {
        multiplier = Mathf.Clamp01(multiplier);
        if (sfxSources != null && sfxBaseVolumes != null)
        {
            for (int i = 0; i < sfxSources.Length; i++)
            {
                if (sfxSources[i] == null) continue;
                sfxSources[i].volume = sfxBaseVolumes[i] * multiplier;
            }
        }
        PlayerData.SetFloat(SFX_VOLUME_KEY, multiplier);
    }

    public void SetParticlesEnabled(bool enabled)
    {
        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (particleSystems[i] == null) continue;
                if (enabled)
                {
                    if (particlesWerePlaying != null && i < particlesWerePlaying.Length && particlesWerePlaying[i])
                        particleSystems[i].Play();
                    if (particlesWerePlaying != null && i < particlesWerePlaying.Length)
                        particlesWerePlaying[i] = false;
                }
                else
                {
                    if (particlesWerePlaying != null && i < particlesWerePlaying.Length)
                        particlesWerePlaying[i] = particleSystems[i].isPlaying;
                    particleSystems[i].Stop();
                }
            }
        }
    }

    public float GetBgmVolume()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return 1f;
        return PlayerData.HasKey(local, BGM_VOLUME_KEY) ? PlayerData.GetFloat(local, BGM_VOLUME_KEY) : 1f;
    }

    public float GetSfxVolume()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return 1f;
        return PlayerData.HasKey(local, SFX_VOLUME_KEY) ? PlayerData.GetFloat(local, SFX_VOLUME_KEY) : 1f;
    }

    public bool GetParticlesEnabled()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return true;
        return PlayerData.HasKey(local, PARTICLES_KEY) ? PlayerData.GetBool(local, PARTICLES_KEY) : true;
    }
}
