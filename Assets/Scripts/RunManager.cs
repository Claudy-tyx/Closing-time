
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [SerializeField] private RunData runData;

    public RunData Data => runData;

    public bool IsRunActive { get; private set; }
    public int LastRunTrashCoins { get; private set; }
    public int LastRunTrashBonus { get; private set; }
    public int LastRunTrashTotal { get; private set; }

    [Header("Playthrough Statistics")]
    [SerializeField] private int totalRunsStarted;
    [SerializeField] private int totalCoinsCollected;

    private float playthroughStartTime;
    private float accumulatedPlaythroughTime;
    private bool playthroughStarted;
    private bool playthroughFinished;

    public int TotalRunsStarted => totalRunsStarted;
    public int TotalCoinsCollected => totalCoinsCollected;

    public float TotalDistanceWalked =>
        runData != null ? runData.DistanceWalked : 0f;

    public float TotalPlaythroughTime
    {
        get
        {
            if (!playthroughStarted)
                return 0f;

            if (playthroughFinished)
                return accumulatedPlaythroughTime;

            return Time.realtimeSinceStartup - playthroughStartTime;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (runData == null)
            Debug.LogError("RunManager: RunData is not assigned!");
    }

    public void BeginNewRun()
    {
        if (runData == null || IsRunActive)
            return;

        BeginPlaythrough();

        totalRunsStarted++;

        runData.StartNewRun();

        LastRunTrashCoins = 0;
        LastRunTrashBonus = 0;
        LastRunTrashTotal = 0;

        IsRunActive = true;

        Debug.Log("New run started!");
    }

    public void EndRun()
    {
        if (runData == null || !IsRunActive)
            return;

        IsRunActive = false;

        LastRunTrashCoins = runData.PendingTrashCoins;

        // Luck level 0-4 = 0%, 25%, 50%, 75%, 100%.
        int luckLevel =
            runData.GetDistanceLevel(DistanceUpgrade.Luck);

        float bonusMultiplier = luckLevel * 0.25f;

        LastRunTrashBonus = Mathf.RoundToInt(
            LastRunTrashCoins * bonusMultiplier
        );

        // Converts the original trash value.
        int converted = runData.ConvertTrashToCoins();

        // Adds the Luck bonus separately.
        if (LastRunTrashBonus > 0)
            runData.AddCoins(LastRunTrashBonus);

        LastRunTrashTotal =
            converted + LastRunTrashBonus;

        Debug.Log(
            $"Run ended! Trash: {converted}, " +
            $"Luck bonus: {LastRunTrashBonus}, " +
            $"Total earned: {LastRunTrashTotal}"
        );
    }

    public void RecordCoinsCollected(int amount)
    {
        if (amount <= 0 || !playthroughStarted || playthroughFinished)
            return;

        totalCoinsCollected += amount;
    }

    public void FinishPlaythrough()
    {
        if (!playthroughStarted || playthroughFinished)
            return;

        accumulatedPlaythroughTime =
            Time.realtimeSinceStartup - playthroughStartTime;

        playthroughFinished = true;
    }

    public void ResetPlaythroughStatistics()
    {
        totalRunsStarted = 0;
        totalCoinsCollected = 0;

        playthroughStartTime = 0f;
        accumulatedPlaythroughTime = 0f;

        playthroughStarted = false;
        playthroughFinished = false;
    }

    public void BeginPlaythrough()
    {
        if (playthroughStarted)
            return;

        playthroughStarted = true;
        playthroughFinished = false;
        playthroughStartTime = Time.realtimeSinceStartup;
    }
}
