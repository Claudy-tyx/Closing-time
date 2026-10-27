
using UnityEngine;

public class CafeCounterNPC : InteractableObject
{
    [Header("Cafe UI")]
    [SerializeField] private CafeShopUI cafeShopUI;

    [Header("NPC Dialogue")]
    [TextArea(2, 4)]
    [SerializeField] private string welcomeMessage =
        "Welcome! What would you like to eat?";

    protected override void Interact()
    {
        if (cafeShopUI == null)
        {
            Debug.LogWarning("CafeShopUI not assigned!");
            return;
        }

        // Open the food selection menu.
        cafeShopUI.Open();

        // Welcome dialogue will be displayed through
        // your existing speech bubble system.
        Debug.Log(welcomeMessage);
    }
}
