using UnityEngine;

public static class BlacksmithData
{
    public const string EXP_KEY = "blacksmith_exp";
    public const string GOLD_KEY = "blacksmith_gold";
    public const string FORGE_LEVEL_KEY = "blacksmith_forge_level";
    public const float EXP_PER_LEVEL = 1000f;

    public static int GetLevel(float exp)
    {
        return Mathf.FloorToInt(exp / EXP_PER_LEVEL) + 1;
    }

    public static float ExpForLevel(int level)
    {
        int l = Mathf.Max(1, level) - 1;
        return l * EXP_PER_LEVEL;
    }

    public static float ProgressToNextLevel(float exp)
    {
        int currentLevel = GetLevel(exp);
        float currentThreshold = ExpForLevel(currentLevel);
        float nextThreshold = ExpForLevel(currentLevel + 1);
        return Mathf.InverseLerp(currentThreshold, nextThreshold, exp);
    }
}
