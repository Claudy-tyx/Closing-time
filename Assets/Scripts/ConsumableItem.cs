
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewConsumable",
    menuName = "Game/Consumable Item"
)]
public class ConsumableItem : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    public Sprite itemIcon;

    [TextArea(2, 4)]
    public string description;

    [Header("Recovery Values")]
    [Min(0)] public int heartsRestored = 0;
    [Min(0)] public float consciousnessRestored = 0f;
    [Min(0)] public float staminaRestored = 0f;
}
