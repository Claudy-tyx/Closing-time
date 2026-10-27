
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DoorApproachTrigger : MonoBehaviour
{
    [SerializeField] private AnimatedDoor door;

    private void Awake()
    {
        if (door == null)
            door = GetComponentInParent<AnimatedDoor>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door?.SetPlayerNearby(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door?.SetPlayerNearby(false);
    }
}
