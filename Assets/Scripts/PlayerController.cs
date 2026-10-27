
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Speed Upgrade")]
    [SerializeField] private float speedBonusPerLevel = 0.3f;

    [Header("Balance Upgrade")]
    [SerializeField] private float maxSwayStrength = 0.65f;
    [SerializeField] private float swayFrequency = 2f;
    [SerializeField] private float swayReductionPerLevel = 0.25f;

    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float staminaDrainPerSecond = 25f;
    [SerializeField] private float staminaRegenPerSecond = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;

    [Header("Distance Tracking")]
    [SerializeField] private float metresPerUnityUnit = 10f;
    [SerializeField] private float maxMovementPerPhysicsStep = 1f;

    private Rigidbody2D rb;
    private PlayerStun playerStun;
    private PlayerSlip playerSlip;

    [Header("Wall Bump SFX")]
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private float wallBumpCooldown = 0.5f;
    [SerializeField] private float wallBumpDirectionThreshold = 0.5f;

    private PlayerSFX playerSFX;
    private float nextWallBumpTime;

    private Vector2 movement;
    private Vector2 previousPosition;

    private RunData runData;

    private bool sprintHeld;
    private float lastSprintTime = float.NegativeInfinity;

    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStun = GetComponent<PlayerStun>();
        playerSlip = GetComponent<PlayerSlip>();
        playerSFX = GetComponent<PlayerSFX>();
    }

    private void OnEnable()
    {
        if (rb != null)
            previousPosition = rb.position;
    }

    private void Start()
    {
        previousPosition = rb.position;

        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;
    }

    private void Update()
    {
        movement = Vector2.zero;
        sprintHeld = false;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            movement.y += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            movement.y -= 1f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            movement.x -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            movement.x += 1f;

        movement = movement.normalized;

        sprintHeld = Keyboard.current.leftShiftKey.isPressed;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Only play during an active run.
        if (!IsRunActive())
            return;

        // Ignore objects outside the selected wall layers.
        if ((wallLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        // Player must be actively trying to move.
        if (movement.sqrMagnitude < 0.01f)
            return;

        // Ignore collisions while slipping or stunned.
        if (playerSlip != null && playerSlip.IsSlipping)
            return;

        if (playerStun != null && playerStun.IsStunned)
            return;

        // Prevent sound spam.
        if (Time.time < nextWallBumpTime)
            return;

        // Check whether the player is pushing into the obstacle.
        foreach (ContactPoint2D contact in collision.contacts)
        {
            float pushAmount = Vector2.Dot(
                movement,
                -contact.normal
            );

            if (pushAmount < wallBumpDirectionThreshold)
                continue;

            if (playerSFX != null)
                playerSFX.PlayWallBump();

            nextWallBumpTime = Time.time + wallBumpCooldown;
            break;
        }
    }

    private void FixedUpdate()
    {
        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        bool runActive = IsRunActive();

        // Track actual movement distance.
        Vector2 currentPosition = rb.position;

        float movedUnits = Vector2.Distance(
            previousPosition,
            currentPosition
        );

        previousPosition = currentPosition;

        if (runData != null &&
            runActive &&
            movement.sqrMagnitude > 0f &&
            movedUnits > 0f &&
            movedUnits <= maxMovementPerPhysicsStep)
        {
            runData.AddDistance(
                movedUnits * metresPerUnityUnit
            );
        }

        // Puddle slipping overrides player movement.
        if (runActive &&
            playerSlip != null &&
            playerSlip.IsSlipping)
        {
            IsSprinting = false;
            RegenerateStamina(runActive);

            rb.linearVelocity = playerSlip.SlipVelocity;
            return;
        }

        // Recovery stun overrides player movement.
        if (runActive &&
            playerStun != null &&
            playerStun.IsStunned)
        {
            IsSprinting = false;
            RegenerateStamina(runActive);

            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calculate walking speed using Speed upgrade.
        int speedLevel = runData != null
            ? runData.GetDistanceLevel(DistanceUpgrade.Speed)
            : 0;

        float currentSpeed =
            moveSpeed + speedLevel * speedBonusPerLevel;

        // Sprint only works during active runs,
        // after purchasing the Sprint unlock.
        IsSprinting =
            runActive &&
            runData != null &&
            runData.SprintUnlocked &&
            sprintHeld &&
            movement.sqrMagnitude > 0f &&
            runData.CurrentSprintStamina > 0f;

        if (IsSprinting)
        {
            float staminaUsed = Mathf.Min(
                staminaDrainPerSecond * Time.fixedDeltaTime,
                runData.CurrentSprintStamina
            );

            runData.ChangeSprintStamina(-staminaUsed);

            lastSprintTime = Time.time;

            currentSpeed *= sprintMultiplier;
        }
        else
        {
            RegenerateStamina(runActive);
        }

        Vector2 finalMovement = movement;

        // Balance sway applies only during active runs.
        if (runActive && movement.sqrMagnitude > 0f)
        {
            int balanceLevel = runData != null
                ? runData.GetDistanceLevel(DistanceUpgrade.Balance)
                : 0;

            float swayMultiplier = Mathf.Clamp01(
                1f - balanceLevel * swayReductionPerLevel
            );

            float swayAmount =
                Mathf.Sin(Time.time * swayFrequency)
                * maxSwayStrength
                * swayMultiplier;

            Vector2 sidewaysDirection = new Vector2(
                -movement.y,
                movement.x
            );

            finalMovement += sidewaysDirection * swayAmount;
            finalMovement = finalMovement.normalized;
        }

        rb.linearVelocity = finalMovement * currentSpeed;
    }

    private void RegenerateStamina(bool runActive)
    {
        if (!runActive ||
            runData == null ||
            !runData.SprintUnlocked)
            return;

        if (Time.time - lastSprintTime < staminaRegenDelay)
            return;

        if (runData.CurrentSprintStamina >=
            runData.MaxSprintStamina)
            return;

        runData.ChangeSprintStamina(
            staminaRegenPerSecond * Time.fixedDeltaTime
        );
    }

    private bool IsRunActive()
    {
        return RunManager.Instance != null &&
               RunManager.Instance.IsRunActive;
    }
}
