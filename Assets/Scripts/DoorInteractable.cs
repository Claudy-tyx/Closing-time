
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string interiorSceneName = "ShopInterior";

    [Header("Return Location")]
    [SerializeField] private Transform returnPoint;

    [Header("Animated Door (Optional)")]
    [SerializeField] private AnimatedDoor animatedDoor;

    private bool playerInside;
    private bool hasTriggered;

    private void Awake()
    {
        if (animatedDoor == null)
            animatedDoor = GetComponentInParent<AnimatedDoor>();

        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        TryEnter();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        TryEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void TryEnter()
    {
        if (!playerInside || hasTriggered)
            return;

        if (animatedDoor != null && !animatedDoor.IsOpen)
            return;

        SceneTransitionManager manager =
            SceneTransitionManager.Instance;

        if (manager == null || manager.IsTransitioning)
            return;

        Vector3 position = returnPoint != null
            ? returnPoint.position
            : transform.position;

        hasTriggered = true;

        manager.EnterInterior(
            interiorSceneName,
            position
        );
    }
}
