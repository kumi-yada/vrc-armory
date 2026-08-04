using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Personal : UdonSharpBehaviour
{
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI forgeLevelText;

    [Header("Upgrade")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeText;

    void Start()
    {
        if (Networking.LocalPlayer == null) return;
        Refresh();
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        if (player == null || !player.isLocal) return;
        Refresh();
    }

    public void Refresh()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return;

        float exp = PlayerData.GetFloat(local, BlacksmithData.EXP_KEY);
        int level = BlacksmithData.GetLevel(exp);
        if (levelText != null)
            levelText.text = "Player Lv." + level;

        int gold = PlayerData.GetInt(local, BlacksmithData.GOLD_KEY);
        if (goldText != null)
            goldText.text = gold + "g";

        int forgeLevel = Mathf.Max(1, (int)PlayerData.GetFloat(local, BlacksmithData.FORGE_LEVEL_KEY));
        if (forgeLevelText != null)
            forgeLevelText.text = "Forge Lv." + forgeLevel;

        RefreshUpgradeButton();
    }

    public void RefreshUpgradeButton()
    {
        Forge forge = GetForge();
        if (forge == null) return;

        if (upgradeButton != null)
            upgradeButton.interactable = forge.CanUpgrade();

        if (upgradeText == null) return;
        if (forge.IsMaxLevel())
            upgradeText.text = "Max Level";
        else
            upgradeText.text = "Upgrade (" + forge.GetNextUpgradeCost() + "g)";
    }

    private Forge GetForge()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (!Utilities.IsValid(local)) return null;

        GameObject[] playerObjects = Networking.GetPlayerObjects(local);
        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (!Utilities.IsValid(playerObjects[i])) continue;
            Forge forge = playerObjects[i].GetComponentInChildren<Forge>();
            if (Utilities.IsValid(forge)) return forge;
        }

        return null;
    }
}
