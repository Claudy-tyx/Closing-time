using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReachedHomeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIPanelAnimator panelAnimator;

    [Header("Statistics")]
    [SerializeField] private TMP_Text totalTimeText;
    [SerializeField] private TMP_Text totalRunsText;
    [SerializeField] private TMP_Text totalDistanceText;
    [SerializeField] private TMP_Text totalCoinsText;

    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button returnPubButton;

    [Header("Scenes")]
    [SerializeField] private string startMenuScene = "StartMenu";
    [SerializeField] private string pubScene = "PubInterior";

    private bool victoryShown;

    private void Awake()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (returnPubButton != null)
            returnPubButton.onClick.AddListener(ReturnToPub);
    }

    public void ShowVictory()
    {
        if (victoryShown)
            return;

        victoryShown = true;

        RunManager manager = RunManager.Instance;

        if (manager != null)
        {
            manager.FinishPlaythrough();

            float seconds = manager.TotalPlaythroughTime;

            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt(seconds / 60f) % 60;
            int secs = Mathf.FloorToInt(seconds) % 60;

            totalTimeText.text =
                $"TOTAL TIME\n{hours:00}:{minutes:00}:{secs:00}";

            totalRunsText.text =
                $"RUNS TAKEN\n{manager.TotalRunsStarted}";

            totalDistanceText.text =
                $"DISTANCE WALKED\n{manager.TotalDistanceWalked:F0} m";

            totalCoinsText.text =
                $"COINS COLLECTED\n{manager.TotalCoinsCollected}";
        }

        panelAnimator.Open();

        Time.timeScale = 0f;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startMenuScene);
    }

    private void ReturnToPub()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(pubScene);
    }
}