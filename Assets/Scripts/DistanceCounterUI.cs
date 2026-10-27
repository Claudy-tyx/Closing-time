
using TMPro;
using UnityEngine;

public class DistanceCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private bool showAvailableDistance = true;

    private RunData runData;

    private void OnEnable()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (runData != null)
            runData.OnDataChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.OnDataChanged -= Refresh;
    }

    private void Refresh()
    {
        if (distanceText == null)
            return;

        if (runData == null)
        {
            distanceText.text = "0 m";
            return;
        }

        if (showAvailableDistance)
            distanceText.text =
                $"{runData.DistanceAvailable:N0} m";
        else
            distanceText.text =
                $"{runData.DistanceWalked:N0} m";
    }
}
