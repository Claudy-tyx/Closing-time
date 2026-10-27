
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float movementThreshold = 0.1f;

    private Rigidbody2D rb;
    private PlayerStun playerStun;
    private PlayerSlip playerSlip;

    private Vector2 facingDirection = Vector2.down;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int FacingXHash =
        Animator.StringToHash("FacingX");

    private static readonly int FacingYHash =
        Animator.StringToHash("FacingY");

    private static readonly int InteractHash =
        Animator.StringToHash("Interact");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStun = GetComponent<PlayerStun>();
        playerSlip = GetComponent<PlayerSlip>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (animator != null)
        {
            animator.SetFloat(FacingXHash, facingDirection.x);
            animator.SetFloat(FacingYHash, facingDirection.y);
            animator.SetFloat(SpeedHash, 0f);
        }
    }

    private void Update()
    {
        if (animator == null)
            return;

        Vector2 velocity = rb.linearVelocity;

        bool isMoving =
            velocity.sqrMagnitude >
            movementThreshold * movementThreshold;

        if (playerStun != null && playerStun.IsStunned)
            isMoving = false;

        if (playerSlip != null && playerSlip.IsSlipping)
            isMoving = false;

        if (isMoving)
        {
            // Select a single cardinal direction.
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                facingDirection = velocity.x > 0f
                    ? Vector2.right
                    : Vector2.left;
            }
            else
            {
                facingDirection = velocity.y > 0f
                    ? Vector2.up
                    : Vector2.down;
            }
        }

        // Preserve last facing direction while idle.
        animator.SetFloat(FacingXHash, facingDirection.x);
        animator.SetFloat(FacingYHash, facingDirection.y);

        animator.SetFloat(SpeedHash, isMoving ? 1f : 0f);
    }

    public void PlayInteraction()
    {
        if (animator == null)
        {
            Debug.LogWarning(
                "PlayerAnimationController: Animator missing!"
            );
            return;
        }

        Debug.Log("Player interaction animation triggered!");

        animator.SetTrigger(InteractHash);
    }
}
