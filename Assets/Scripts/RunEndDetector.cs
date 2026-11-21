using UnityEngine;

public class RunEndDetector : MonoBehaviour
{
    [SerializeField] private RunGameOverUI gameOverUI;

    private RunData runData;

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (gameOverUI == null)
            gameOverUI =
                FindAnyObjectByType<RunGameOverUI>();
    }

    private void Update()
    {
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log(
                $"[END DEBUG] Detector running. " +
                $"Hearts={runData?.CurrentHearts}, " +
                $"Consciousness={runData?.CurrentConsciousness}"
            );
        }
        RunManager manager = RunManager.Instance;

        if (manager == null || !manager.IsRunActive)
            return;

        if (runData == null)
            runData = manager.Data;

        if (runData == null)
            return;

        if (runData.CurrentHearts <= 0)
        {
            EndRun(GetHeartFailureReason(manager.LastDamageCause));
        }
        else if (runData.CurrentConsciousness <= 0f)
        {
            EndRun("You lost consciousness!");
        }
    }

    private string GetHeartFailureReason(DamageCause cause)
    {
        switch (cause)
        {
            case DamageCause.Car:
                return "You were hit by a car!";

            case DamageCause.Pedestrian:
                return "You bumped into a pedestrian!";

            case DamageCause.CatAllergy:
                return "Your allergies got the better of you!";

            case DamageCause.Environmental:
                return "You were injured by an obstacle!";

            default:
                return "You collapsed from your injuries!";
        }
    }

    private void EndRun(string reason)
    {
        RunManager manager = RunManager.Instance;

        if (manager == null || !manager.IsRunActive)
            return;

        // Record failure before ending the run.
        manager.RecordFailedRun();

        // EndRun makes IsRunActive false, preventing duplicate counts.
        manager.EndRun();

        Debug.Log(
            $"Run failed! Total failures: {manager.TotalFailedRuns}"
        );

        if (gameOverUI != null)
            gameOverUI.ShowGameOver(reason);
        else
            Debug.LogWarning(
                "RunEndDetector: Game Over UI is missing!"
            );
    }
}