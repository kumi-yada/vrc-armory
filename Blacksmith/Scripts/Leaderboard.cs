using UdonSharp;
using UnityEngine;
using TMPro;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Leaderboard : UdonSharpBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI localPlayerEntry;
    [SerializeField] private string localPlayerFormat = "You: Lv.{0} ({1} XP)";

    [Header("Ranks")]
    [SerializeField] private TextMeshProUGUI[] rankNameTexts;
    [SerializeField] private TextMeshProUGUI[] rankLevelTexts;
    [SerializeField] private GameObject[] rankRows;

    [Header("Settings")]
    [SerializeField] private float refreshInterval = 10f;
    [SerializeField] private int maxEntries = 8;

    private float timer;

    void Start()
    {
        if (Networking.LocalPlayer == null) return;
        Refresh();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            Refresh();
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Refresh();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Refresh();
    }

    public void Refresh()
    {
        VRCPlayerApi[] players = VRCPlayerApi.GetPlayers();
        if (players == null || players.Length == 0)
        {
            ClearEntries();
            return;
        }

        int count = players.Length;
        if (count > maxEntries)
            count = maxEntries;

        string[] names = new string[players.Length];
        int[] levels = new int[players.Length];
        float[] exps = new float[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            VRCPlayerApi player = players[i];
            if (!Utilities.IsValid(player)) continue;

            float exp = PlayerData.GetFloat(player, BlacksmithData.EXP_KEY);
            int level = BlacksmithData.GetLevel(exp);

            names[i] = player.displayName;
            levels[i] = level;
            exps[i] = exp;
        }

        SortEntries(names, levels, exps);

        if (localPlayerEntry != null)
        {
            VRCPlayerApi local = Networking.LocalPlayer;
            float localExp = PlayerData.GetFloat(local, BlacksmithData.EXP_KEY);
            int localLevel = BlacksmithData.GetLevel(localExp);
            localPlayerEntry.text = string.Format(localPlayerFormat, localLevel, Mathf.FloorToInt(localExp));
        }

        int displayCount = count;
        if (displayCount > rankNameTexts.Length)
            displayCount = rankNameTexts.Length;
        if (displayCount > rankLevelTexts.Length)
            displayCount = rankLevelTexts.Length;

        for (int i = 0; i < displayCount; i++)
        {
            if (rankRows != null && i < rankRows.Length && rankRows[i] != null)
                rankRows[i].SetActive(true);

            if (rankNameTexts[i] != null)
                rankNameTexts[i].text = names[i];
            if (rankLevelTexts[i] != null)
                rankLevelTexts[i].text = string.Format("Lv.{0}  ({1} XP)", levels[i], Mathf.FloorToInt(exps[i]));
        }

        for (int i = displayCount; i < (rankRows != null ? rankRows.Length : 0); i++)
        {
            if (rankRows[i] != null)
                rankRows[i].SetActive(false);
        }
    }

    private void ClearEntries()
    {
        if (localPlayerEntry != null)
            localPlayerEntry.text = "No players";

        for (int i = 0; i < rankNameTexts.Length; i++)
        {
            if (rankNameTexts[i] != null)
                rankNameTexts[i].text = "";
            if (rankLevelTexts[i] != null)
                rankLevelTexts[i].text = "";
        }

        if (rankRows != null)
        {
            for (int i = 0; i < rankRows.Length; i++)
            {
                if (rankRows[i] != null)
                    rankRows[i].SetActive(false);
            }
        }
    }

    private void SortEntries(string[] names, int[] levels, float[] exps)
    {
        for (int i = 1; i < levels.Length; i++)
        {
            string keyName = names[i];
            int keyLevel = levels[i];
            float keyExp = exps[i];
            int j = i - 1;
            while (j >= 0 && levels[j] < keyLevel)
            {
                names[j + 1] = names[j];
                levels[j + 1] = levels[j];
                exps[j + 1] = exps[j];
                j--;
            }
            names[j + 1] = keyName;
            levels[j + 1] = keyLevel;
            exps[j + 1] = keyExp;
        }
    }
}
