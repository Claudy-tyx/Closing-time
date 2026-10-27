
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCWander : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider2D wanderArea;
    [SerializeField] private NPCDirectionalAnimator npcAnimator;
    [SerializeField] private NPCBumpReaction bumpReaction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float arrivalDistance = 0.1f;

    [Header("Idle")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    private Rigidbody2D rb;
    private Vector2 destination;
    private float idleTimer;
    private bool isIdle = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (npcAnimator == null)
            npcAnimator = GetComponent<NPCDirectionalAnimator>();

        if (bumpReaction == null)
            bumpReaction = GetComponent<NPCBumpReaction>();
    }

    private void Start()
    {
        StartIdle();
    }

    private void FixedUpdate()
    {
        if (bumpReaction != null && bumpReaction.IsBumping)
        {
            npcAnimator?.UpdateAnimation(Vector2.zero);
            return;
        }

        if (isIdle)
        {
            idleTimer -= Time.fixedDeltaTime;
            npcAnimator?.UpdateAnimation(Vector2.zero);

            if (idleTimer <= 0f)
                ChooseDestination();

            return;
        }

        Vector2 difference = destination - rb.position;

        if (difference.magnitude <= arrivalDistance)
        {
            StartIdle();
            return;
        }

        Vector2 movement =
            difference.normalized *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(
            Vector2.MoveTowards(
                rb.position,
                destination,
                moveSpeed * Time.fixedDeltaTime
            )
        );

        npcAnimator?.UpdateAnimation(movement);
    }

    private void StartIdle()
    {
        isIdle = true;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
        npcAnimator?.UpdateAnimation(Vector2.zero);
    }

    private void ChooseDestination()
    {
        if (wanderArea == null)
        {
            StartIdle();
            return;
        }

        Bounds bounds = wanderArea.bounds;

        destination = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        isIdle = false;
    }
}
