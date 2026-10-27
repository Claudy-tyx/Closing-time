
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button distanceTabButton;
    [SerializeField] private Button coinsTabButton;
    [SerializeField] private GameObject distanceUpgradesPanel;
    [SerializeField] private GameObject coinUpgradesPanel;

    [Header("Currency Counters")]
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text coinsText;

    [Header("Upgrade Rows")]
    [SerializeField] private UpgradeRowUI[] upgradeRows;

    [Header("Panel Animation")]
    [SerializeField] private UIPanelAnimator panelAnimator;

    private RunData data;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (distanceTabButton != null)
            distanceTabButton.onClick.AddListener(() => ShowTab(true));

        if (coinsTabButton != null)
            coinsTabButton.onClick.AddListener(() => ShowTab(false));
    }

    private void Start()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager missing!");
            return;
        }

        data = RunManager.Instance.Data;

        if (data == null)
        {
            Debug.LogError("RunData missing!");
            return;
        }

        foreach (UpgradeRowUI row in upgradeRows)
        {
            if (row != null)
                row.Initialize(data);
        }

        data.OnDataChanged += RefreshAll;

        RefreshAll();
        ShowTab(true);
    }

    private void OnDestroy()
    {
        if (data != null)
            data.OnDataChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        if (data == null)
            return;

        if (distanceText != null)
            distanceText.text = $"{data.DistanceAvailable} m";

        if (coinsText != null)
            coinsText.text = $"{data.Coins}";

        foreach (UpgradeRowUI row in upgradeRows)
        {
            if (row != null && row.isActiveAndEnabled)
                row.Refresh();
        }
    }

    private void ShowTab(bool showDistance)
    {
        if (distanceUpgradesPanel != null)
            distanceUpgradesPanel.SetActive(showDistance);

        if (coinUpgradesPanel != null)
            coinUpgradesPanel.SetActive(!showDistance);

        RefreshAll();
    }

    public void Open()
    {
        if (panelAnimator != null)
            panelAnimator.Open();

        ShowTab(true);
        RefreshAll();
    }

    public void Close()
    {
        if (panelAnimator != null)
            panelAnimator.Close();
    }
}
