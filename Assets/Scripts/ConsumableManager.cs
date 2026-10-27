
using UnityEngine;

public class ConsumableManager : MonoBehaviour
{
    public static ConsumableManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool UseConsumable(ConsumableItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("Consumable item is missing!");
            return false;
        }

        if (RunManager.Instance == null ||
            RunManager.Instance.Data == null ||
            !RunManager.Instance.IsRunActive)
        {
            Debug.LogWarning(
                "Cannot use consumable without an active run."
            );
            return false;
        }

        RunData data = RunManager.Instance.Data;

        float healingMultiplier =
            data.EnhancedHealingUnlocked ? 2f : 1f;

        int hearts = Mathf.RoundToInt(
            item.heartsRestored * healingMultiplier
        );

        float consciousness =
            item.consciousnessRestored * healingMultiplier;

        float stamina = item.staminaRestored;

        if (hearts > 0)
            data.ChangeHearts(hearts);

        if (consciousness > 0f)
            data.ChangeConsciousness(consciousness);

        if (stamina > 0f)
            data.ChangeSprintStamina(stamina);

        Debug.Log(
            $"Used {item.itemName}: " +
            $"+{hearts} hearts, " +
            $"+{consciousness} consciousness, " +
            $"+{stamina} stamina."
        );

        return true;
    }
}
