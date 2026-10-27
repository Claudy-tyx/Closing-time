
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class DoorEntranceTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnimatedDoor animatedDoor;

    [Header("Options")]
    [SerializeField] private bool requireOpenDoor = true;
    [SerializeField] private bool oneTimeActivation = true;

    [Header("On Enter")]
    [SerializeField] private UnityEvent onPlayerEnter;

    private bool activated;

    private void Awake()
    {
        if (animatedDoor == null)
            animatedDoor = GetComponentInParent<AnimatedDoor>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryEnter(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryEnter(other);
    }

    
    private void TryEnter(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (oneTimeActivation && activated)
            return;

        if (requireOpenDoor &&
            (animatedDoor == null || !animatedDoor.IsOpen))
            return;

        activated = true;

        // Begin existing door transition.
        onPlayerEnter?.Invoke();
    }
}
