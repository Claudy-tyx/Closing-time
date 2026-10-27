
using System;
using System.Collections.Generic;
using UnityEngine;

public enum DistanceUpgrade
{
    Balance,
    Speed,
    Recovery,
    Sight,
    Luck
}

public enum CoinUpgrade
{
    ExtraHeart,
    Sprint,
    EnhancedHealing,
    AllergenImmunity,
    NonSlipShoes,
    Minimap
}

[CreateAssetMenu(fileName = "RunData", menuName = "Game/Run Data")]
public class RunData : ScriptableObject
{
    [Serializable]
    public class CollectedTrashItem
    {
        public string itemName;
        public Sprite itemSprite;
        public int coinValue;
    }

    [Header("Currencies")]
    [SerializeField] private int coins;
    [SerializeField] private int coinsCollected;
    [SerializeField] private float distanceWalked;
    [SerializeField] private int distanceAvailable;
    [SerializeField] private int drinksPurchased;

    [Header("Trash Collected This Run")]
    [SerializeField] private List<CollectedTrashItem> collectedTrash =
        new List<CollectedTrashItem>();

    [Header("Distance Upgrade Levels (0-4)")]
    [Range(0, 4)] [SerializeField] private int balanceLevel;
    [Range(0, 4)] [SerializeField] private int speedLevel;
    [Range(0, 4)] [SerializeField] private int recoveryLevel;
    [Range(0, 4)] [SerializeField] private int sightLevel;
    [Range(0, 4)] [SerializeField] private int luckLevel;

    [Header("Consciousness")]
    [SerializeField] private int consciousnessPurchases;
    [SerializeField] private int baseConsciousness = 10;
    [SerializeField] private float currentConsciousness;

    [Header("Coin Unlockables")]
    [SerializeField] private int extraHeartsPurchased;
    [SerializeField] private bool sprintUnlocked;
    [SerializeField] private bool enhancedHealingUnlocked;
    [SerializeField] private bool allergenImmunityUnlocked;
    [SerializeField] private bool nonSlipShoesUnlocked;

    [SerializeField] private bool minimapUnlocked;

    [Header("Current Meters")]
    [SerializeField] private int baseHearts = 3;
    [SerializeField] private int currentHearts;
    [SerializeField] private float currentSprintStamina;
    [SerializeField] private float maxSprintStamina = 100f;

    [Header("Distance Upgrade Costs (Level 1-4)")]
    [SerializeField] private int[] distanceUpgradeCosts =
        { 150, 500, 1000, 3000 };

    [Header("Coin Upgrade Costs")]
    [SerializeField] private int firstExtraHeartCost = 20;
    [SerializeField] private int extraHeartCostIncrease = 10;
    [SerializeField] private int sprintCost = 40;
    [SerializeField] private int enhancedHealingCost = 40;
    [SerializeField] private int allergenImmunityCost = 50;
    [SerializeField] private int nonSlipShoesCost = 50;

    [SerializeField] private int minimapCost = 40;

    public event Action OnDataChanged;

    // CURRENCIES

    public int Coins => coins;
    public int CoinsCollected => coinsCollected;
    public int DrinksPurchased => drinksPurchased;

    public float DistanceWalked => distanceWalked;
    public int DistanceAvailable => distanceAvailable;

    // CONSCIOUSNESS

    public int ConsciousnessPurchases =>
        consciousnessPurchases;

    public float CurrentConsciousness =>
        currentConsciousness;

    public int MaxConsciousness =>
        baseConsciousness + consciousnessPurchases * 10;

    // HEARTS AND STAMINA

    public int CurrentHearts => currentHearts;
    public int MaxHearts => baseHearts + extraHeartsPurchased;

    public float CurrentSprintStamina =>
        currentSprintStamina;

    public float MaxSprintStamina =>
        maxSprintStamina;

    // UNLOCKABLES

    public int ExtraHeartsPurchased =>
        extraHeartsPurchased;

    public bool SprintUnlocked => sprintUnlocked;
    public bool EnhancedHealingUnlocked =>
        enhancedHealingUnlocked;

    public bool AllergenImmunityUnlocked =>
        allergenImmunityUnlocked;

    public bool NonSlipShoesUnlocked =>
        nonSlipShoesUnlocked;

    public bool MinimapUnlocked => minimapUnlocked;

    // LUCK: 0%, 25%, 50%, 75%, 100%

    public float TrashLuckBonus =>
        Mathf.Clamp(luckLevel, 0, 4) * 0.25f;

    // TRASH INVENTORY

    public IReadOnlyList<CollectedTrashItem> CollectedTrash =>
        collectedTrash;

    public int TrashItemCount =>
        collectedTrash == null ? 0 : collectedTrash.Count;

    public int PendingTrashCoins
    {
        get
        {
            int total = 0;

            if (collectedTrash == null)
                return total;

            foreach (CollectedTrashItem item in collectedTrash)
            {
                if (item != null)
                    total += Mathf.Max(0, item.coinValue);
            }

            return total;
        }
    }

    public int PendingTrashBonus =>
        Mathf.RoundToInt(PendingTrashCoins * TrashLuckBonus);

    public int PendingTrashTotal =>
        PendingTrashCoins + PendingTrashBonus;

    public void CollectTrashItem(
        string itemName,
        Sprite itemSprite,
        int coinValue)
    {
        if (collectedTrash == null)
            collectedTrash = new List<CollectedTrashItem>();

        collectedTrash.Add(new CollectedTrashItem
        {
            itemName = itemName,
            itemSprite = itemSprite,
            coinValue = Mathf.Max(0, coinValue)
        });

        NotifyChanged();
    }

    // Converts BASE trash value only.
    // RunManager.EndRun() awards the Luck bonus separately.
    public int ConvertTrashToCoins()
    {
        if (collectedTrash == null ||
            collectedTrash.Count == 0)
            return 0;

        int earned = PendingTrashCoins;

        // Clearing first prevents duplicate conversion.
        collectedTrash.Clear();

        if (earned > 0)
            AddCoins(earned);
        else
            NotifyChanged();

        return earned;
    }

    // COINS

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        coins += amount;
        coinsCollected += amount;

        RunManager.Instance?.RecordCoinsCollected(amount);

        NotifyChanged();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0 || coins < amount)
            return false;

        coins -= amount;
        NotifyChanged();

        return true;
    }

    public void RecordDrinkPurchase()
    {
        drinksPurchased++;
        NotifyChanged();
    }

    // DISTANCE

    // Input amount is measured in metres.
    // PlayerDistanceTracker converts Unity units to metres.
    public void AddDistance(float amount)
    {
        if (amount <= 0f)
            return;

        float previousDistance = distanceWalked;

        distanceWalked += amount;

        int earned =
            Mathf.FloorToInt(distanceWalked) -
            Mathf.FloorToInt(previousDistance);

        distanceAvailable += earned;

        NotifyChanged();
    }

    public bool SpendDistance(int amount)
    {
        if (amount < 0 || distanceAvailable < amount)
            return false;

        distanceAvailable -= amount;
        NotifyChanged();

        return true;
    }

    // DISTANCE UPGRADES

    public int GetDistanceLevel(DistanceUpgrade upgrade)
    {
        switch (upgrade)
        {
            case DistanceUpgrade.Balance:
                return balanceLevel;

            case DistanceUpgrade.Speed:
                return speedLevel;

            case DistanceUpgrade.Recovery:
                return recoveryLevel;

            case DistanceUpgrade.Sight:
                return sightLevel;

            case DistanceUpgrade.Luck:
                return luckLevel;

            default:
                return 0;
        }
    }

    public int GetDistanceUpgradeCost(
        DistanceUpgrade upgrade)
    {
        int level = GetDistanceLevel(upgrade);

        if (level >= 4)
            return -1;

        if (distanceUpgradeCosts == null ||
            distanceUpgradeCosts.Length <= level)
            return -1;

        return distanceUpgradeCosts[level];
    }

    public bool BuyDistanceUpgrade(
        DistanceUpgrade upgrade)
    {
        int cost = GetDistanceUpgradeCost(upgrade);

        if (cost < 0 || distanceAvailable < cost)
            return false;

        distanceAvailable -= cost;

        switch (upgrade)
        {
            case DistanceUpgrade.Balance:
                balanceLevel++;
                break;

            case DistanceUpgrade.Speed:
                speedLevel++;
                break;

            case DistanceUpgrade.Recovery:
                recoveryLevel++;
                break;

            case DistanceUpgrade.Sight:
                sightLevel++;
                break;

            case DistanceUpgrade.Luck:
                luckLevel++;
                break;
        }

        NotifyChanged();
        return true;
    }

    // CONSCIOUSNESS UPGRADES

    // +10 maximum consciousness per purchase.
    // Costs 50, 70, 90, 110, ...
    public int GetConsciousnessCost()
    {
        return 50 + consciousnessPurchases * 20;
    }

    public bool BuyConsciousness()
    {
        int cost = GetConsciousnessCost();

        if (distanceAvailable < cost)
            return false;

        distanceAvailable -= cost;
        consciousnessPurchases++;

        // New maximum applies when the next run starts.
        NotifyChanged();
        return true;
    }

    // COIN UPGRADES

    public int GetCoinUpgradeLevel(CoinUpgrade upgrade)
    {
        switch (upgrade)
        {
            case CoinUpgrade.ExtraHeart:
                return extraHeartsPurchased;

            case CoinUpgrade.Sprint:
                return sprintUnlocked ? 1 : 0;

            case CoinUpgrade.EnhancedHealing:
                return enhancedHealingUnlocked ? 1 : 0;

            case CoinUpgrade.AllergenImmunity:
                return allergenImmunityUnlocked ? 1 : 0;

            case CoinUpgrade.NonSlipShoes:
                return nonSlipShoesUnlocked ? 1 : 0;

            case CoinUpgrade.Minimap:
                return minimapUnlocked ? 1 : 0;

            default:
                return 0;
        }
    }

    public int GetCoinUpgradeCost(CoinUpgrade upgrade)
    {
        switch (upgrade)
        {
            case CoinUpgrade.ExtraHeart:
                return firstExtraHeartCost +
                    extraHeartsPurchased *
                    extraHeartCostIncrease;

            case CoinUpgrade.Sprint:
                return sprintUnlocked ? -1 : sprintCost;

            case CoinUpgrade.EnhancedHealing:
                return enhancedHealingUnlocked
                    ? -1 : enhancedHealingCost;

            case CoinUpgrade.AllergenImmunity:
                return allergenImmunityUnlocked
                    ? -1 : allergenImmunityCost;

            case CoinUpgrade.NonSlipShoes:
                return nonSlipShoesUnlocked
                    ? -1 : nonSlipShoesCost;

            case CoinUpgrade.Minimap:
                return minimapUnlocked ? -1 : minimapCost;

            default:
                return -1;
        }
    }

    public bool BuyCoinUpgrade(CoinUpgrade upgrade)
    {
        int cost = GetCoinUpgradeCost(upgrade);

        if (cost < 0 || coins < cost)
            return false;

        coins -= cost;

        switch (upgrade)
        {
            case CoinUpgrade.ExtraHeart:
                extraHeartsPurchased++;
                break;

            case CoinUpgrade.Sprint:
                sprintUnlocked = true;
                break;

            case CoinUpgrade.EnhancedHealing:
                enhancedHealingUnlocked = true;
                break;

            case CoinUpgrade.AllergenImmunity:
                allergenImmunityUnlocked = true;
                break;

            case CoinUpgrade.NonSlipShoes:
                nonSlipShoesUnlocked = true;
                break;

            case CoinUpgrade.Minimap:
                minimapUnlocked = true;
                break;
        }

        NotifyChanged();
        return true;
    }

    // CURRENT METERS

    public void ChangeConsciousness(float amount)
    {
        float previousConsciousness = currentConsciousness;

        currentConsciousness = Mathf.Clamp(
            currentConsciousness + amount,
            0f,
            MaxConsciousness
        );

        // Play healing effect only when consciousness increases.
        if (currentConsciousness > previousConsciousness)
        {
            PlayerScreenEffects.Instance?.PlayHeal();
        }

        NotifyChanged();
    }

    public void ChangeHearts(int amount)
    {
        int previousHearts = currentHearts;

        currentHearts = Mathf.Clamp(
            currentHearts + amount,
            0,
            MaxHearts
        );

        if (currentHearts < previousHearts)
        {
            PlayerScreenEffects.Instance?.PlayHurt();
        }
        else if (currentHearts > previousHearts)
        {
            PlayerScreenEffects.Instance?.PlayHeal();
        }

        NotifyChanged();
    }

    public void ChangeSprintStamina(float amount)
    {
        currentSprintStamina = Mathf.Clamp(
            currentSprintStamina + amount,
            0f,
            maxSprintStamina
        );

        NotifyChanged();
    }

    // RUN MANAGEMENT

    // Called when the player leaves the pub.
    public void StartNewRun()
    {
        currentConsciousness = MaxConsciousness;
        currentHearts = MaxHearts;
        currentSprintStamina = maxSprintStamina;

        coinsCollected = 0;
        drinksPurchased = 0;

        if (collectedTrash == null)
            collectedTrash = new List<CollectedTrashItem>();
        else
            collectedTrash.Clear();

        // Total distance and available distance are preserved.
        // Purchased upgrades and coin balance are preserved.

        NotifyChanged();
    }

    // Completely reset progression for a new save.
    public void ResetAllProgress()
    {
        coins = 0;
        coinsCollected = 0;
        drinksPurchased = 0;

        distanceWalked = 0f;
        distanceAvailable = 0;

        balanceLevel = 0;
        speedLevel = 0;
        recoveryLevel = 0;
        sightLevel = 0;
        luckLevel = 0;

        consciousnessPurchases = 0;

        extraHeartsPurchased = 0;
        sprintUnlocked = false;
        enhancedHealingUnlocked = false;
        allergenImmunityUnlocked = false;
        nonSlipShoesUnlocked = false;
        minimapUnlocked = false;

        if (collectedTrash != null)
            collectedTrash.Clear();

        RunManager.Instance?.ResetPlaythroughStatistics();

        StartNewRun();
    }

    // Compatibility with existing scripts.
    public void ResetRun()
    {
        StartNewRun();
    }

    private void NotifyChanged()
    {
        OnDataChanged?.Invoke();
    }
}
