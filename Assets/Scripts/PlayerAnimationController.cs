
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Animation Playback Speed")]
    [Tooltip("Animation playback speed multiplier in Section 1 (step-by-step staggering walking).")]
    [SerializeField] private float section1AnimSpeed = 1.0f;

    [Tooltip("Animation playback speed multiplier at the start of Section 2 (transition threshold).")]
    [SerializeField] private float section2StartAnimSpeed = 1.35f;

    [Tooltip("Maximum animation playback speed multiplier at 100% runaway tipping speed.")]
    [SerializeField] private float section2MaxAnimSpeed = 2.2f;

    [Tooltip("Extra speed multiplier applied when sprinting.")]
    [SerializeField] private float sprintAnimMultiplier = 1.25f;

    [Tooltip("Animation playback speed multiplier when counter-braking (rapid catching steps).")]
    [SerializeField] private float brakingAnimSpeed = 1.85f;

    public float Section1AnimSpeed
    {
        get => section1AnimSpeed;
        set => section1AnimSpeed = Mathf.Max(0.2f, value);
    }

    public float Section2StartAnimSpeed
    {
        get => section2StartAnimSpeed;
        set => section2StartAnimSpeed = Mathf.Max(0.5f, value);
    }

    public float Section2MaxAnimSpeed
    {
        get => section2MaxAnimSpeed;
        set => section2MaxAnimSpeed = Mathf.Max(0.5f, value);
    }

    public float CurrentAnimPlaybackSpeed => animator != null ? animator.speed : 1f;

    private Rigidbody2D rb;
    private PlayerStun playerStun;
    private PlayerSlip playerSlip;
    private PlayerController playerController;

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
        playerController = GetComponent<PlayerController>();

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
        Vector2 input = playerController != null ? playerController.MovementInput : Vector2.zero;

        bool hasInput = input.sqrMagnitude > 0.01f;
        float speedSqr = velocity.sqrMagnitude;
        bool hasVelocity = speedSqr > movementThreshold * movementThreshold;

        bool isMoving = hasInput || hasVelocity;

        if (playerStun != null && playerStun.IsStunned)
            isMoving = false;

        if (playerSlip != null && playerSlip.IsSlipping)
            isMoving = false;

        if (playerController != null && (playerController.IsFallingSkid || playerController.IsPostSkidImmobile))
            isMoving = false;

        bool isReverseBraking = hasInput && speedSqr > 0.001f && Vector2.Dot(input, velocity) < -0.05f;

        if (isMoving)
        {
            Vector2 checkVec = velocity;

            if (isReverseBraking)
            {
                // Counter-braking: face the original travel direction instead of reverse input
                checkVec = facingDirection;
            }
            else if (hasInput)
            {
                // Active driving: face intentional input so sway wobble doesn't flicker facing direction
                checkVec = input;
            }

            // Select a single cardinal direction.
            if (Mathf.Abs(checkVec.x) > Mathf.Abs(checkVec.y))
            {
                facingDirection = checkVec.x > 0f
                    ? Vector2.right
                    : Vector2.left;
            }
            else
            {
                facingDirection = checkVec.y > 0f
                    ? Vector2.up
                    : Vector2.down;
            }
        }

        // Preserve last facing direction while idle.
        animator.SetFloat(FacingXHash, facingDirection.x);
        animator.SetFloat(FacingYHash, facingDirection.y);

        animator.SetFloat(SpeedHash, isMoving ? 1f : 0f);

        // Dynamic animation playback speed scaling:
        // Section 1: deliberate step-by-step pace
        // Section 2: rapid leg movement speeding up as runaway momentum accelerates
        if (!isMoving)
        {
            animator.speed = 1.0f;
        }
        else if (playerController != null)
        {
            float targetSpeed = section1AnimSpeed;

            if (isReverseBraking || playerController.IsCounterBraking)
            {
                // Rapid catching steps when braking to bleed off momentum
                targetSpeed = brakingAnimSpeed;
            }
            else if (!playerController.IsInFirstSection)
            {
                // In Section 2: legs pump faster as runaway tipping speed increases
                float lean = playerController.LeanProgress;
                float threshold = playerController.TransitionLeanThreshold;
                float section2Ratio = Mathf.Clamp01((lean - threshold) / Mathf.Max(0.01f, 1.0f - threshold));
                targetSpeed = Mathf.Lerp(section2StartAnimSpeed, section2MaxAnimSpeed, section2Ratio);
            }

            if (playerController.IsSprinting)
            {
                targetSpeed *= sprintAnimMultiplier;
            }

            animator.speed = targetSpeed;
        }
        else
        {
            animator.speed = 1.0f;
        }
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
