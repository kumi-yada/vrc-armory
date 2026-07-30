using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Leaderboard : UdonSharpBehaviour
{
    [Header("Rows")]
    [SerializeField] private LeaderRow[] rows;

    void Start()
    {
        if (Networking.LocalPlayer == null) return;
        Refresh();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Refresh();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Refresh();
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
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

        int displayCount = players.Length;
        if (displayCount > rows.Length)
            displayCount = rows.Length;

        for (int i = 0; i < displayCount; i++)
            rows[i].SetEntry(names[i], levels[i], exps[i]);

        for (int i = displayCount; i < rows.Length; i++)
            rows[i].Clear();
    }

    private void ClearEntries()
    {
        for (int i = 0; i < rows.Length; i++)
            rows[i].Clear();
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
