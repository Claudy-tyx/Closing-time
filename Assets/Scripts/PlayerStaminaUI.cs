
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image staminaFill;

    [Header("Animation")]
    [SerializeField] private float fillSmoothSpeed = 8f;

    private RunData runData;
    private float displayedFill = 1f;

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        RefreshImmediate();
    }

    private void Update()
    {
        if (runData == null && RunManager.Instance != null)
        {
            runData = RunManager.Instance.Data;
            RefreshImmediate();
        }

        if (runData == null || staminaFill == null)
            return;

        bool showStamina =
            runData.SprintUnlocked &&
            RunManager.Instance != null &&
            RunManager.Instance.IsRunActive;

        // Hide until Sprint is purchased and a run is active.
        if (staminaFill.transform.parent != null)
        {
            // Visibility is handled by the CanvasGroup
            // or the bar's root Image.
        }

        float targetFill = runData.MaxSprintStamina > 0f
            ? runData.CurrentSprintStamina /
              runData.MaxSprintStamina
            : 0f;

        displayedFill = Mathf.Lerp(
            displayedFill,
            targetFill,
            fillSmoothSpeed * Time.unscaledDeltaTime
        );

        staminaFill.fillAmount = displayedFill;

        SetBarVisibility(showStamina);
    }

    private void RefreshImmediate()
    {
        if (runData == null || staminaFill == null)
            return;

        displayedFill = runData.MaxSprintStamina > 0f
            ? runData.CurrentSprintStamina /
              runData.MaxSprintStamina
            : 0f;

        staminaFill.fillAmount = displayedFill;
    }

    private void SetBarVisibility(bool visible)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();

        if (group == null)
            return;

        group.alpha = visible ? 1f : 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
