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

        PlayerEntry[] entries = new PlayerEntry[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            VRCPlayerApi player = players[i];
            if (!Utilities.IsValid(player)) continue;

            float exp = PlayerData.GetFloat(player, BlacksmithData.EXP_KEY);
            int level = BlacksmithData.GetLevel(exp);
            string displayName = player.displayName;

            entries[i] = new PlayerEntry
            {
                displayName = displayName,
                level = level,
                exp = exp,
                isLocal = player.isLocal
            };
        }

        SortEntries(entries);

        if (localPlayerEntry != null)
        {
            VRCPlayerApi local = Networking.LocalPlayer;
            float localExp = PlayerData.GetFloat(local, BlacksmithData.EXP_KEY);
            int localLevel = BlacksmithData.GetLevel(localExp);
            localPlayerEntry.text = string.Format(localPlayerFormat, localLevel, Mathf.FloorToInt(localExp));
        }

        int displayCount = Mathf.Min(count, rankNameTexts.Length, rankLevelTexts.Length);
        for (int i = 0; i < displayCount; i++)
        {
            if (rankRows != null && i < rankRows.Length && rankRows[i] != null)
                rankRows[i].SetActive(true);

            PlayerEntry entry = entries[i];
            if (rankNameTexts[i] != null)
                rankNameTexts[i].text = entry.displayName;
            if (rankLevelTexts[i] != null)
                rankLevelTexts[i].text = string.Format("Lv.{0}  ({1} XP)", entry.level, Mathf.FloorToInt(entry.exp));
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

    private void SortEntries(PlayerEntry[] entries)
    {
        for (int i = 1; i < entries.Length; i++)
        {
            PlayerEntry key = entries[i];
            int j = i - 1;
            while (j >= 0 && entries[j].level < key.level)
            {
                entries[j + 1] = entries[j];
                j--;
            }
            entries[j + 1] = key;
        }
    }

    private struct PlayerEntry
    {
        public string displayName;
        public int level;
        public float exp;
        public bool isLocal;
    }
}
