
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class VendingMachineUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [Header("Optional Item Display")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image itemIconImage;

    [Header("World SFX")]
    [SerializeField] private AudioClip vendingOpenClip;
    [SerializeField] private AudioClip buySuccessClip;
    [SerializeField] private AudioMixerGroup worldSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float vendingOpenVolume = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float buySuccessVolume = 0.8f;

    private AudioSource worldSFXSource;

    private RunData runData;
    private ConsumableItem currentConsumable;
    private int currentPrice;
    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        worldSFXSource = gameObject.AddComponent<AudioSource>();

        worldSFXSource.playOnAwake = false;
        worldSFXSource.loop = false;
        worldSFXSource.spatialBlend = 0f;
        worldSFXSource.outputAudioMixerGroup = worldSFXMixerGroup;

        panel.SetActive(false);

        buyButton.onClick.AddListener(BuyDrink);
        closeButton.onClick.AddListener(Close);
    }

    private void PlayWorldSFX(AudioClip clip, float volume)
    {
        if (clip == null || worldSFXSource == null)
            return;

        worldSFXSource.PlayOneShot(clip, volume);
    }

    public void Open(int price, ConsumableItem consumable)
    {
        if (RunManager.Instance == null ||
            RunManager.Instance.Data == null)
        {
            Debug.LogError("RunManager or RunData missing!");
            return;
        }

        if (consumable == null)
        {
            Debug.LogWarning("No consumable assigned!");
            return;
        }

        runData = RunManager.Instance.Data;
        currentPrice = price;
        currentConsumable = consumable;

        panel.SetActive(true);
        RunConsciousnessTimer.Pause(this);
        PlayWorldSFX(vendingOpenClip, vendingOpenVolume);

        if (itemNameText != null)
            itemNameText.text = currentConsumable.itemName;

        if (itemIconImage != null)
        {
            itemIconImage.sprite = currentConsumable.itemIcon;
            itemIconImage.enabled = currentConsumable.itemIcon != null;
        }

        if (messageText != null)
            messageText.text = currentConsumable.description;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (runData == null)
            return;

        if (priceText != null)
            priceText.text = $"Price: {currentPrice}";

        if (balanceText != null)
            balanceText.text = $"Balance: {runData.Coins}";

        if (buyButton != null)
            buyButton.interactable =
                currentConsumable != null &&
                runData.Coins >= currentPrice;
    }

    private void BuyDrink()
    {
        if (runData == null || currentConsumable == null)
            return;

        ConsumableManager manager =
            FindAnyObjectByType<ConsumableManager>();

        if (manager == null)
        {
            Debug.LogError(
                "ConsumableManager missing! Purchase cancelled."
            );
            return;
        }

        if (runData.Coins < currentPrice)
        {
            if (messageText != null)
                messageText.text = "Not enough coins!";

            UpdateDisplay();
            return;
        }

        if (!runData.SpendCoins(currentPrice))
            return;
        PlayWorldSFX(buySuccessClip, buySuccessVolume);

        // Apply the consumable immediately.
        manager.UseConsumable(currentConsumable);

        runData.RecordDrinkPurchase();

        if (messageText != null)
        {
            messageText.text =
                $"{currentConsumable.itemName} purchased and consumed!";
        }

        UpdateDisplay();

        Debug.Log(
            $"Consumed {currentConsumable.itemName} " +
            $"for {currentPrice} coins."
        );
    }

    public void Close()
    {
        RunConsciousnessTimer.Resume(this);

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDisable()
    {
        RunConsciousnessTimer.Resume(this);
    }

    private void OnDestroy()
    {
        RunConsciousnessTimer.Resume(this);
    }
}
