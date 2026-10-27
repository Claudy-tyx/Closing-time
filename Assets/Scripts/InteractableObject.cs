
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    private Transform player;
    private bool playerInRange;

    protected bool IsPlayerInRange => playerInRange;
    protected Transform PlayerTransform => player;

    protected virtual void Awake()
    {
        FindPlayer();
        SetHighlight(false);
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    protected virtual void Update()
    {
        if (player == null)
            FindPlayer();

        if (player == null)
        {
            SetHighlight(false);
            playerInRange = false;
            return;
        }

        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        bool inRange = distance <= interactionRange;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            SetHighlight(playerInRange);
        }
    }

    public void TryInteract()
    {
        if (!playerInRange)
            return;

        Interact();
    }

    protected abstract void Interact();

    private void SetHighlight(bool active)
    {
        if (highlightObject != null &&
            highlightObject.activeSelf != active)
        {
            highlightObject.SetActive(active);
        }
    }

    protected virtual void OnDisable()
    {
        SetHighlight(false);
        playerInRange = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}
