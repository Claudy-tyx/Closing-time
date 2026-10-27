
using TMPro;
using UnityEngine;

public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private RunData runData;

    private void OnEnable()
    {
        if (runData != null)
            runData.OnDataChanged += UpdateDisplay;

        UpdateDisplay();
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.OnDataChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (coinText == null || runData == null)
            return;

        coinText.text = $"{runData.Coins}";
    }
}
