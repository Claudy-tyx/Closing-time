
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerConsciousnessUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image consciousnessFill;
    [SerializeField] private TMP_Text consciousnessText;

    [Header("Animation")]
    [SerializeField] private float fillSmoothSpeed = 8f;

    [Header("Bar Colours")]
    [SerializeField] private Color highColor =
        new Color(0.45f, 0.85f, 0.65f);

    [SerializeField] private Color mediumColor =
        new Color(1f, 0.75f, 0.3f);

    [SerializeField] private Color lowColor =
        new Color(0.95f, 0.3f, 0.3f);

    private RunData runData;
    private float targetFill;

    private void OnEnable()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (runData != null)
            runData.OnDataChanged += Refresh;

        Refresh();

        if (consciousnessFill != null)
            consciousnessFill.fillAmount = targetFill;
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.OnDataChanged -= Refresh;
    }

    private void Update()
    {
        if (consciousnessFill == null)
            return;

        consciousnessFill.fillAmount = Mathf.MoveTowards(
            consciousnessFill.fillAmount,
            targetFill,
            fillSmoothSpeed * Time.unscaledDeltaTime
        );

        UpdateBarColor(consciousnessFill.fillAmount);
    }

    private void Refresh()
    {
        if (runData == null)
            return;

        float max = Mathf.Max(1f, runData.MaxConsciousness);
        float current = runData.CurrentConsciousness;

        targetFill = Mathf.Clamp01(current / max);

        if (consciousnessText != null)
        {
            consciousnessText.text =
                $"{Mathf.CeilToInt(current)} / " +
                $"{runData.MaxConsciousness}";
        }
    }

    private void UpdateBarColor(float fill)
    {
        if (fill > 0.5f)
        {
            consciousnessFill.color = highColor;
        }
        else if (fill > 0.25f)
        {
            consciousnessFill.color = mediumColor;
        }
        else
        {
            consciousnessFill.color = lowColor;
        }
    }
}
