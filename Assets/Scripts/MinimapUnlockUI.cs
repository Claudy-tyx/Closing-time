
using UnityEngine;

public class MinimapUnlockUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject minimapUI;

    private RunData runData;

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        Refresh();
    }

    private void Update()
    {
        // Supports scene transitions and run state changes.
        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        Refresh();
    }

    private void Refresh()
    {
        if (minimapUI == null)
            return;

        bool shouldShow =
            runData != null &&
            runData.MinimapUnlocked &&
            RunManager.Instance != null &&
            RunManager.Instance.IsRunActive;

        if (minimapUI.activeSelf != shouldShow)
            minimapUI.SetActive(shouldShow);
    }
}
