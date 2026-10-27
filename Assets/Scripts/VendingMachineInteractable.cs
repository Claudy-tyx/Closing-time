
using UnityEngine;

public class VendingMachineInteractable : InteractableObject
{
    [Header("Consumable")]
    [SerializeField] private ConsumableItem consumable;

    [Header("Price")]
    [SerializeField] private int minPrice = 3;
    [SerializeField] private int maxPrice = 8;
    

    private int price;
    private VendingMachineUI vendingUI;
    private bool openedByThisMachine;

    private static VendingMachineInteractable activeMachine;

    public ConsumableItem Consumable => consumable;
    public int Price => price;

    private void Start()
    {
        price = Random.Range(minPrice, maxPrice + 1);
        vendingUI = FindAnyObjectByType<VendingMachineUI>();
    }

    protected override void Interact()
    {
        if (vendingUI == null)
        {
            Debug.LogWarning("VendingMachineUI not found!");
            return;
        }

        if (consumable == null)
        {
            Debug.LogWarning(
                $"No consumable assigned to {gameObject.name}!"
            );
            return;
        }

        if (activeMachine != null && activeMachine != this)
            activeMachine.CloseVendingMachine();

        activeMachine = this;
        openedByThisMachine = true;

        vendingUI.Open(price, consumable);
    }

    protected override void Update()
    {
        base.Update();

        if (!openedByThisMachine)
            return;

        // Player walked away, or closed the UI manually.
        if (!IsPlayerInRange ||
            vendingUI == null ||
            !vendingUI.IsOpen)
        {
            CloseVendingMachine();
        }
    }

    private void CloseVendingMachine()
    {
        if (!openedByThisMachine)
            return;

        openedByThisMachine = false;

        if (activeMachine == this)
            activeMachine = null;

        if (vendingUI != null)
            vendingUI.Close();
    }

    protected override void OnDisable()
    {
        CloseVendingMachine();
        base.OnDisable();
    }
}
