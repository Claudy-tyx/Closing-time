
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CafeShopUI : MonoBehaviour
{
    [Serializable]
    public class FoodEntry
    {
        public ConsumableItem consumable;
        [Min(0)] public int price = 10;
    }

    [Header("Food Menu")]
    [SerializeField] private List<FoodEntry> foodItems =
        new List<FoodEntry>();

    [Header("UI Panel")]
    [SerializeField] private GameObject panel;

    [Header("Food Display")]
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private TMP_Text foodDescriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private Image foodImage;

    [Header("Buttons")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text messageText;

    [Header("Panel Animation")]
    [SerializeField] private UIPanelAnimator panelAnimator;

    private int currentIndex;
    private RunData runData;
    private ConsumableManager consumableManager;

    private void Awake()
    {
        previousButton.onClick.AddListener(PreviousItem);
        nextButton.onClick.AddListener(NextItem);
        buyButton.onClick.AddListener(BuyCurrentItem);
        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        if (runData != null)
        {
            runData.OnDataChanged -= Refresh;
            runData.OnDataChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.OnDataChanged -= Refresh;
    }

    private void OnDestroy()
    {
        if (runData != null)
            runData.OnDataChanged -= Refresh;
    }

    public void Open()
    {
        if (RunManager.Instance == null ||
            RunManager.Instance.Data == null)
        {
            Debug.LogError("CafeShopUI: RunManager missing!");
            return;
        }

        if (runData != null)
            runData.OnDataChanged -= Refresh;

        runData = RunManager.Instance.Data;

        consumableManager =
            FindAnyObjectByType<ConsumableManager>();

        if (consumableManager == null)
        {
            Debug.LogWarning(
                "CafeShopUI: ConsumableManager not found."
            );
        }

        runData.OnDataChanged += Refresh;

        currentIndex = 0;

        if (panelAnimator != null)
            panelAnimator.Open();

        if (messageText != null)
            messageText.text = "";

        Refresh();
    }

    public void Close()
    {
        if (panelAnimator != null)
            panelAnimator.Close();
    }

    private void PreviousItem()
    {
        if (foodItems.Count == 0)
            return;

        currentIndex =
            (currentIndex - 1 + foodItems.Count) %
            foodItems.Count;

        ClearMessage();
        Refresh();
    }

    private void NextItem()
    {
        if (foodItems.Count == 0)
            return;

        currentIndex = (currentIndex + 1) % foodItems.Count;

        ClearMessage();
        Refresh();
    }

    private void Refresh()
    {
        if (foodItems.Count == 0)
        {
            if (foodNameText != null)
                foodNameText.text = "No food available";

            if (foodDescriptionText != null)
                foodDescriptionText.text = "";

            if (priceText != null)
                priceText.text = "";

            if (foodImage != null)
                foodImage.enabled = false;

            if (buyButton != null)
                buyButton.interactable = false;

            if (previousButton != null)
                previousButton.interactable = false;

            if (nextButton != null)
                nextButton.interactable = false;

            return;
        }

        currentIndex = Mathf.Clamp(
            currentIndex, 0, foodItems.Count - 1
        );

        FoodEntry entry = foodItems[currentIndex];
        ConsumableItem item = entry.consumable;

        if (foodNameText != null)
        {
            foodNameText.text =
                item != null ? item.itemName : "Missing Item";
        }

        if (foodDescriptionText != null)
        {
            foodDescriptionText.text =
                item != null ? item.description : "";
        }

        if (priceText != null)
            priceText.text = $"Price: {entry.price}";

        if (balanceText != null && runData != null)
            balanceText.text = $"Balance: {runData.Coins}";

        if (foodImage != null)
        {
            foodImage.sprite =
                item != null ? item.itemIcon : null;

            foodImage.enabled =
                item != null && item.itemIcon != null;
        }

        if (previousButton != null)
            previousButton.interactable = foodItems.Count > 1;

        if (nextButton != null)
            nextButton.interactable = foodItems.Count > 1;

        if (buyButton != null)
        {
            buyButton.interactable =
                item != null &&
                runData != null &&
                consumableManager != null &&
                runData.Coins >= entry.price;
        }
    }

    
    private void BuyCurrentItem()
    {
        if (runData == null ||
            consumableManager == null ||
            foodItems.Count == 0)
            return;

        FoodEntry entry = foodItems[currentIndex];
        ConsumableItem item = entry.consumable;

        if (item == null)
            return;

        if (!RunManager.Instance.IsRunActive)
            return;

        if (runData.Coins < entry.price)
        {
            foodDescriptionText.text = "Not enough coins!";
            return;
        }

        // Record actual stats before consumption.
        int heartsBefore = runData.CurrentHearts;
        float consciousnessBefore = runData.CurrentConsciousness;
        float staminaBefore = runData.CurrentSprintStamina;

        if (!runData.SpendCoins(entry.price))
            return;

        // Apply consumable, including Enhanced Healing.
        consumableManager.UseConsumable(item);

        // Calculate the actual amount restored.
        int heartsRestored =
            runData.CurrentHearts - heartsBefore;

        float consciousnessRestored =
            runData.CurrentConsciousness - consciousnessBefore;

        float staminaRestored =
            runData.CurrentSprintStamina - staminaBefore;

        // Update coins and button availability.
        Refresh();

        string result = "Purchased and eaten!";
        UIPurchaseSFX.Instance?.PlayPurchase();

        if (heartsRestored > 0)
            result += $"\nRestored {heartsRestored} hearts.";

        if (consciousnessRestored > 0f)
            result +=
                $"\nRestored {consciousnessRestored:0.#} consciousness.";

        if (staminaRestored > 0f)
            result +=
                $"\nRestored {staminaRestored:0.#} stamina.";

        if (heartsRestored <= 0 &&
            consciousnessRestored <= 0f &&
            staminaRestored <= 0f)
        {
            result += "\nAll stats are already full!";
        }

        if (foodDescriptionText != null)
            foodDescriptionText.text = result;
    }


    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}
