
using UnityEngine;
using UnityEngine.UI;

public class ResetGameDataButton : MonoBehaviour
{
    [SerializeField] private Button resetButton;

    private void Awake()
    {
        if (resetButton == null)
            resetButton = GetComponent<Button>();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGameData);
    }

    private void OnDestroy()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetGameData);
    }

    private void ResetGameData()
    {
        if (RunManager.Instance == null ||
            RunManager.Instance.Data == null)
        {
            Debug.LogWarning("RunData not found!");
            return;
        }

        RunManager.Instance.Data.ResetAllProgress();

        Debug.Log("Game data reset!");
    }
}
