using UnityEngine;

public static class BlacksmithData
{
    public const string EXP_KEY = "blacksmith_exp";
    public const string GOLD_KEY = "blacksmith_gold";

    public static int GetLevel(float exp)
    {
        return Mathf.FloorToInt(Mathf.Sqrt(exp / 100f)) + 1;
    }

    public static float ExpForLevel(int level)
    {
        int l = Mathf.Max(1, level) - 1;
        return l * l * 100f;
    }

    public static float ProgressToNextLevel(float exp)
    {
        int currentLevel = GetLevel(exp);
        float currentThreshold = ExpForLevel(currentLevel);
        float nextThreshold = ExpForLevel(currentLevel + 1);
        return Mathf.InverseLerp(currentThreshold, nextThreshold, exp);
    }
}
