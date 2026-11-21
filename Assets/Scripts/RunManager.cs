
using UnityEngine;
using UnityEngine.InputSystem;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [SerializeField] private RunData runData;

    public RunData Data => runData;

    public bool IsRunActive { get; private set; }

    public int LastRunTrashCoins { get; private set; }
    public int LastRunTrashBonus { get; private set; }
    public int LastRunTrashTotal { get; private set; }

    // Compatibility properties for existing scripts.

    public int TotalRunsStarted =>
        runData != null ? runData.TotalRunsStarted : 0;

    public int TotalFailedRuns =>
        runData != null ? runData.TotalFailedRuns : 0;

    public int TotalCoinsCollected =>
        runData != null ? runData.TotalCoinsCollected : 0;

    public float TotalDistanceWalked =>
        runData != null ? runData.DistanceWalked : 0f;

    public float TotalPlaythroughTime =>
        runData != null ? runData.TotalPlaythroughTime : 0f;

    public bool FirstUpgradeTutorialCompleted =>
        runData != null && runData.FirstUpgradeTutorialCompleted;

    public bool ThirdFailureRewardClaimed =>
        runData != null && runData.ThirdFailureRewardClaimed;

    public bool FifthFailureRewardClaimed =>
        runData != null && runData.FifthFailureRewardClaimed;

    public bool PubMapUnlocked =>
        runData != null && runData.PubMapUnlocked;

    public DamageCause LastDamageCause =>
        runData != null
            ? runData.LastDamageCause
            : DamageCause.Unknown;

    public void SetRunData(RunData data)
    {
        runData = data;
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
        {
            runData = Resources.Load<RunData>("RunData");
            if (runData == null)
            {
                Debug.LogWarning("RunManager: RunData is not assigned. Ensure it is assigned or SetRunData() is called before starting a run.");
            }
        }
    }

    private void LateUpdate()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "NewCity")
            return;

        if (Time.frameCount % 120 != 0)
            return;

        Debug.Log(
            $"[RUN DEBUG] " +
            $"Active={IsRunActive}, " +
            $"Scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, " +
            $"Hearts={runData?.CurrentHearts}, " +
            $"Consciousness={runData?.CurrentConsciousness}, " +
            $"TimerPaused={RunConsciousnessTimer.IsPaused}"
        );
    }

    public void BeginNewRun()
    {
        if (runData == null || IsRunActive)
            return;

        runData.BeginPlaythrough();
        runData.ClearDamageCause();
        runData.RecordRunStarted();

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

        int luckLevel =
            runData.GetDistanceLevel(DistanceUpgrade.Luck);

        float bonusMultiplier = luckLevel * 0.25f;

        LastRunTrashBonus = Mathf.RoundToInt(
            LastRunTrashCoins * bonusMultiplier
        );

        int converted = runData.ConvertTrashToCoins();

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

    public void RecordFailedRun()
    {
        if (!IsRunActive || runData == null)
            return;

        runData.RecordFailedRun();
    }

    public void RecordDamageCause(DamageCause cause)
    {
        runData?.RecordDamageCause(cause);
    }

    public void RecordCoinsCollected(int amount)
    {
        runData?.RecordCoinsCollected(amount);
    }

    public void BeginPlaythrough()
    {
        runData?.BeginPlaythrough();
    }

    public void FinishPlaythrough()
    {
        runData?.FinishPlaythrough();
    }

    public void CompleteFirstUpgradeTutorial()
    {
        runData?.CompleteFirstUpgradeTutorial();
    }

    public void ClaimThirdFailureReward()
    {
        runData?.ClaimThirdFailureReward();
    }

    public void ClaimFifthFailureReward()
    {
        runData?.ClaimFifthFailureReward();
    }

    public void UnlockPubMap()
    {
        runData?.UnlockPubMap();
    }

    public void ResetPlaythroughStatistics()
    {
        runData?.ResetPlaythroughStatistics();

        IsRunActive = false;

        LastRunTrashCoins = 0;
        LastRunTrashBonus = 0;
        LastRunTrashTotal = 0;
    }
}
