using UnityEngine;

namespace Core
{
    public static class SaveLoadManager
    {
        public static int GetLevel() => PlayerPrefs.GetInt("Level", 1);
        public static int GetLevelIndex() => GetLevel() - 1;
        public static void IncreaseLevel() => PlayerPrefs.SetInt("Level", GetLevel() + 1);
    }
}