using UnityEngine;
using UnityEngine.UI;

public class PubMapUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mapButton;
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenMap);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMap);

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (mapButton != null)
            mapButton.SetActive(false);
    }

    private void Start()
    {
        RefreshUnlock();
    }

    public void RefreshUnlock()
    {
        bool unlocked =
            RunManager.Instance != null &&
            RunManager.Instance.PubMapUnlocked;

        if (mapButton != null)
        {
            mapButton.SetActive(unlocked);
        }

        Debug.Log($"Pub map unlocked: {unlocked}");
    }

    public void OpenMap()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.PubMapUnlocked)
            return;

        if (mapPanel != null)
            mapPanel.SetActive(true);
    }

    public void CloseMap()
    {
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }
}