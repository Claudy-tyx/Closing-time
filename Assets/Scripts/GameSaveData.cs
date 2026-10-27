
using UnityEngine;

public static class GameSaveData
{
    private const string BusPassKey = "HasBusPass";

    public static bool HasBusPass =>
        PlayerPrefs.GetInt(BusPassKey, 0) == 1;

    public static void UnlockBusPass()
    {
        if (HasBusPass)
            return;

        PlayerPrefs.SetInt(BusPassKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Bus pass permanently unlocked!");
    }
}
