
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeRowUI : MonoBehaviour
{
    [Header("Upgrade Type")]
    [SerializeField] private bool isCoinUpgrade;
    [SerializeField] private bool isConsciousness;
    [SerializeField] private DistanceUpgrade distanceUpgrade;
    [SerializeField] private CoinUpgrade coinUpgrade;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image levelImage;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text costText;

    [Header("Level Bar PNGs")]
    [SerializeField] private Sprite[] levelSprites = new Sprite[5];

    private RunData data;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(BuyUpgrade);
    }

    public void Initialize(RunData runData)
    {
        if (data != null)
            data.OnDataChanged -= Refresh;

        data = runData;

        if (data != null)
        {
            if (isActiveAndEnabled)
                data.OnDataChanged += Refresh;

            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (data != null)
            data.OnDataChanged -= Refresh;
    }

    public void Refresh()
    {
        if (data == null || upgradeButton == null)
            return;

        if (isConsciousness)
        {
            RefreshConsciousness();
        }
        else if (isCoinUpgrade)
        {
            RefreshCoinUpgrade();
        }
        else
        {
            RefreshDistanceUpgrade();
        }
    }

    private void RefreshConsciousness()
    {
        if (nameText != null)
            nameText.text = "Consciousness";

        if (valueText != null)
            valueText.text =
                $"{data.MaxConsciousness} points";

        int cost = data.GetConsciousnessCost();

        costText.text = $"{cost} m";
        upgradeButton.interactable =
            data.DistanceAvailable >= cost;
    }

    private void RefreshDistanceUpgrade()
    {
        int level = data.GetDistanceLevel(distanceUpgrade);
        int cost = data.GetDistanceUpgradeCost(distanceUpgrade);

        if (nameText != null)
            nameText.text = distanceUpgrade.ToString();

        UpdateLevelDisplay(level, 4);

        costText.text = cost < 0 ? "MAX" : $"{cost} m";

        upgradeButton.interactable =
            cost >= 0 && data.DistanceAvailable >= cost;
    }

    private void RefreshCoinUpgrade()
    {
        int level = data.GetCoinUpgradeLevel(coinUpgrade);
        int cost = data.GetCoinUpgradeCost(coinUpgrade);

        if (nameText != null)
            nameText.text = GetCoinUpgradeName();

        int maxLevel = coinUpgrade == CoinUpgrade.ExtraHeart
            ? -1
            : 1;

        UpdateLevelDisplay(level, maxLevel);

        costText.text = cost < 0 ? "OWNED" : $"{cost}";

        upgradeButton.interactable =
            cost >= 0 && data.Coins >= cost;
    }

    private void OnDisable()
    {
        if (data != null)
            data.OnDataChanged -= Refresh;
    }

    private void OnEnable()
    {
        if (data != null)
        {
            data.OnDataChanged -= Refresh;
            data.OnDataChanged += Refresh;
            Refresh();
        }
    }

    private void UpdateLevelDisplay(int level, int maxLevel)
    {
        if (valueText != null)
        {
            valueText.text = maxLevel < 0
                ? $"+{level} hearts"
                : $"Lv. {level}/{maxLevel}";
        }

        if (levelImage != null &&
            levelSprites != null &&
            levelSprites.Length > 0)
        {
            int index = Mathf.Clamp(
                level,
                0,
                levelSprites.Length - 1
            );

            if (levelSprites[index] != null)
                levelImage.sprite = levelSprites[index];
        }
    }

    private string GetCoinUpgradeName()
    {
        switch (coinUpgrade)
        {
            case CoinUpgrade.ExtraHeart:
                return "Extra Heart";
            case CoinUpgrade.Sprint:
                return "Sprint";
            case CoinUpgrade.EnhancedHealing:
                return "Enhanced Healing";
            case CoinUpgrade.AllergenImmunity:
                return "Allergen Immunity";
            case CoinUpgrade.NonSlipShoes:
                return "Non-Slip Shoes";
            case CoinUpgrade.Minimap:
                return "Minimap";
            default:
                return coinUpgrade.ToString();
        }
    }

    private void BuyUpgrade()
    {
        if (data == null)
            return;

        bool purchased;

        if (isConsciousness)
            purchased = data.BuyConsciousness();
        else if (isCoinUpgrade)
            purchased = data.BuyCoinUpgrade(coinUpgrade);
        else
            purchased = data.BuyDistanceUpgrade(distanceUpgrade);

        if (purchased)
        {
            Refresh();
            UIPurchaseSFX.Instance?.PlayPurchase();
            Debug.Log($"Upgrade purchased: {gameObject.name}");
        }
    }
}
