
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class RunGameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private TMP_Text rewardsText;

    [Header("Scene")]
    [SerializeField] private string pubSceneName = "PubInterior";

    private bool gameOverActive;
    private bool waitingForMouseRelease;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (!gameOverActive || Mouse.current == null)
            return;

        // Avoid immediately leaving if the mouse was held
        // when the run ended.
        if (waitingForMouseRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
                waitingForMouseRelease = false;

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            ReturnToPub();
    }

    public void ShowGameOver(string reason)
    {
        if (gameOverActive)
            return;

        gameOverActive = true;
        waitingForMouseRelease = true;

        if (reasonText != null)
            reasonText.text = reason;

        if (RunManager.Instance != null)
        {
            RunManager manager = RunManager.Instance;

            if (rewardsText != null)
            {
                rewardsText.text =
                    $"Trash collected: {manager.LastRunTrashCoins} coins\n" +
                    $"Luck bonus: +{manager.LastRunTrashBonus} coins\n" +
                    $"Total earned: {manager.LastRunTrashTotal} coins";
            }
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Replace the current BGM with Game Over music.
        GameBGMManager.Instance?.PlayGameOver();

        Time.timeScale = 0f;
    }

    private void ReturnToPub()
    {
        if (!gameOverActive)
            return;

        gameOverActive = false;

        // Restore normal gameplay before loading another scene.
        Time.timeScale = 1f;

        SceneManager.LoadScene(pubSceneName);
    }

    private void OnDestroy()
    {
        // Safety: never leave Unity paused if this UI is destroyed.
        if (gameOverActive)
            Time.timeScale = 1f;
    }
}
