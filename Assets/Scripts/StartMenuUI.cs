using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string pubSceneName = "PubInterior";

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button closeControlsButton;

    [Header("Controls Panel")]
    [SerializeField] private UIPanelAnimator controlsAnimator;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);

        if (closeControlsButton != null)
            closeControlsButton.onClick.AddListener(CloseControls);
    }

    private void StartGame()
    {
        Time.timeScale = 1f;

        RunManager.Instance?.BeginPlaythrough();

        SceneManager.LoadScene(pubSceneName);
    }

    private void OpenControls()
    {
        if (controlsAnimator != null)
            controlsAnimator.Open();
    }

    private void CloseControls()
    {
        if (controlsAnimator != null)
            controlsAnimator.Close();
    }
}