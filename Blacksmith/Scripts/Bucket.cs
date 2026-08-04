using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bucket : UdonSharpBehaviour
{
    [Header("Cooling")]
    [SerializeField] private float coolRate = 10f;

    [Header("Steam Particles")]
    [SerializeField] private ParticleSystem steamParticles;
    [SerializeField] private float minEmission = 5f;
    [SerializeField] private float maxEmission = 50f;
    [SerializeField] private int maxTrackedWeapons = 8;

    [Header("Steam Audio")]
    [SerializeField] private AudioSource steamAudio;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    [System.NonSerialized] private SmiteWeapon[] weaponsInBucket;
    [System.NonSerialized] private int weaponCount;

    void Start()
    {
        weaponsInBucket = new SmiteWeapon[maxTrackedWeapons];
        weaponCount = 0;

        if (steamParticles != null)
            steamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (steamAudio != null)
            steamAudio.Stop();
    }

    public void OnTriggerEnter(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        AddWeapon(weapon);

        if (Networking.IsOwner(weapon.gameObject))
        {
            weapon.isHeated = false;
            weapon.coolRate = coolRate;
            weapon.RequestSerialization();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        SmiteWeapon weapon = other.GetComponentInParent<SmiteWeapon>();
        if (!Utilities.IsValid(weapon))
            return;

        RemoveWeapon(weapon);

        if (Networking.IsOwner(weapon.gameObject))
        {
            weapon.ResetCoolRate();
        }
    }

    private void Update()
    {
        float hottest = 0f;
        for (int i = 0; i < weaponCount; i++)
        {
            if (!Utilities.IsValid(weaponsInBucket[i]))
            {
                RemoveWeaponAt(i);
                i--;
                continue;
            }

            hottest = Mathf.Max(hottest, weaponsInBucket[i].GetHeatNormalized());
        }

        float norm = Mathf.Clamp01(hottest);
        bool active = norm > 0f;

        if (steamParticles != null)
        {
            if (!active)
            {
                if (steamParticles.isPlaying)
                    steamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else
            {
                if (!steamParticles.isPlaying)
                    steamParticles.Play();

                var emission = steamParticles.emission;
                emission.rateOverTimeMultiplier = Mathf.Lerp(minEmission, maxEmission, norm);
            }
        }

        if (steamAudio != null)
        {
            if (!active)
            {
                if (steamAudio.isPlaying)
                    steamAudio.Stop();
            }
            else
            {
                if (!steamAudio.isPlaying)
                    steamAudio.Play();

                steamAudio.volume = Mathf.Lerp(minVolume, maxVolume, norm);
                steamAudio.pitch = Mathf.Lerp(minPitch, maxPitch, norm);
            }
        }
    }

    private void AddWeapon(SmiteWeapon weapon)
    {
        for (int i = 0; i < weaponCount; i++)
        {
            if (weaponsInBucket[i] == weapon)
                return;
        }

        if (weaponCount < weaponsInBucket.Length)
        {
            weaponsInBucket[weaponCount] = weapon;
            weaponCount++;
        }
    }

    private void RemoveWeapon(SmiteWeapon weapon)
    {
        for (int i = 0; i < weaponCount; i++)
        {
            if (weaponsInBucket[i] == weapon)
            {
                RemoveWeaponAt(i);
                return;
            }
        }
    }

    private void RemoveWeaponAt(int index)
    {
        for (int i = index; i < weaponCount - 1; i++)
            weaponsInBucket[i] = weaponsInBucket[i + 1];

        weaponCount--;
        weaponsInBucket[weaponCount] = null;
    }
}
