
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitDoorInteractable : MonoBehaviour
{
    private bool hasTriggered;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasTriggered)
            return;

        SceneTransitionManager manager =
            SceneTransitionManager.Instance;

        if (manager == null || manager.IsTransitioning)
            return;

        hasTriggered = true;
        manager.ExitInterior();
    }
}
