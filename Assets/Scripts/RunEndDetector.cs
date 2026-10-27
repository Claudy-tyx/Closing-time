
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
                FindFirstObjectByType<RunGameOverUI>();
    }

    private void Update()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
            return;

        if (runData == null)
            runData = RunManager.Instance.Data;

        if (runData == null)
            return;

        if (runData.CurrentHearts <= 0)
        {
            EndRun("You ran out of hearts!");
        }
        else if (runData.CurrentConsciousness <= 0f)
        {
            EndRun("You lost consciousness!");
        }
    }

    private void EndRun(string reason)
    {
        RunManager.Instance.EndRun();

        if (gameOverUI != null)
            gameOverUI.ShowGameOver(reason);
        else
            Debug.LogWarning(
                "RunEndDetector: Game Over UI is missing!"
            );
    }
}
