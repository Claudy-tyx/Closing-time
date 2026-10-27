using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Collider2D[] hits =
            Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in hits)
        {
            InteractableObject interactable =
                hit.GetComponentInParent<InteractableObject>();

            if (interactable == null)
                continue;

            interactable.TryInteract();
            break;
        }
    }
}