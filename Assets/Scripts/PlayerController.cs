
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Unicycle Speed & Acceleration")]
    [Tooltip("Master speed multiplier (like a master volume slider) that scales all speeds, accelerations, and braking rates proportionally.")]
    [Range(0.2f, 3.0f)]
    [SerializeField] private float masterSpeedScale = 1.0f;

    [Tooltip("Base starting speed.")]
    [SerializeField] private float moveSpeed = 1.4f;

    [Tooltip("Speed bonus added per Speed upgrade level.")]
    [SerializeField] private float speedBonusPerLevel = 0.25f;

    [Tooltip("Acceleration rate in Section 1 (at the start). Matches braking deceleration for symmetrical speed-up and slow-down.")]
    [SerializeField] private float section1Acceleration = 6.5f;

    [Tooltip("Acceleration rate in Section 2 (runaway speed).")]
    [SerializeField] private float section2Acceleration = 3.2f;

    [Header("Turning & Steering Arc")]
    [Tooltip("Turn rate (degrees/sec) at low speed. Higher = sharper, tighter turns.")]
    [SerializeField] private float lowSpeedTurnRate = 260f;

    [Tooltip("Turn rate (degrees/sec) at max runaway speed. Lower = wider, sweeping curves that take time to redirect.")]
    [SerializeField] private float highSpeedTurnRate = 85f;

    [Header("Momentum & Deceleration")]
    [Tooltip("Friction/deceleration when coasting (keys released). Lower = more unicycle glide.")]
    [SerializeField] private float coastingDeceleration = 1.4f;

    [Tooltip("Braking deceleration when pressing opposite direction to stop.")]
    [SerializeField] private float brakingDeceleration = 6.5f;

    [Header("Exponential Speed & Tipping")]
    [Tooltip("Parabolic exponent for runaway speed increase.")]
    [SerializeField] private float speedExponent = 2.0f;

    [Tooltip("Max speed multiplier over base speed at tipping point.")]
    [SerializeField] private float maxSpeedMultiplier = 2.0f;

    [Tooltip("Continuous hold time (seconds) before reaching tipping point (100% lean).")]
    [SerializeField] private float timeToMaxSpeed = 4.8f;

    [Tooltip("Duration (seconds) the player can sustain maximum runaway speed before tipping over and wiping out.")]
    [SerializeField] private float maxTopSpeedDuration = 1.5f;

    [Tooltip("Rate at which directional momentum decays when coasting.")]
    [SerializeField] private float momentumDecayRate = 2.0f;

    [Header("Two-Phase Lean System")]
    [Tooltip("Percentage of tipping speed (0.0 to 1.0) where Section 1 (irregular sway & footsteps) transitions into Section 2 (straight runaway acceleration).")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float transitionLeanThreshold = 0.5f;

    [Tooltip("Strength of the irregular organic sway during Section 1.")]
    [SerializeField] private float section1SwayStrength = 0.16f;

    [Tooltip("Strength of the organic sway during Section 2 runaway tipping.")]
    [SerializeField] private float section2SwayStrength = 0.22f;

    [Tooltip("Whether to enable sway in Section 2 (runaway tipping). Disabled by default per user request, but toggleable.")]
    [SerializeField] private bool enableSection2Sway = false;

    [Tooltip("Minimum duration (seconds) of a single sway pulse (randomized for inconsistent cadence).")]
    [SerializeField] private float minSwayPulseDuration = 0.45f;

    [Tooltip("Maximum duration (seconds) of a single sway pulse (randomized for inconsistent cadence).")]
    [SerializeField] private float maxSwayPulseDuration = 0.85f;

    [Tooltip("Maximum consecutive sways allowed in one direction (Left or Right) before forcing the opposite side.")]
    [SerializeField] private int maxConsecutiveSwaysSameDirection = 3;

    [Header("Step-by-Step Cadence (Section 1)")]
    [Tooltip("Enable step-by-step push/plant velocity modulation in Section 1 (making steps feel discrete rather than a continuous fluid glide).")]
    [SerializeField] private bool enableStepPulse = true;

    [Tooltip("Base duration (seconds) of a single step in Section 1.")]
    [SerializeField] private float stepDuration = 0.42f;

    [Tooltip("Strength of the forward surge during the push-off phase (e.g. 0.28 = +28% speed peak).")]
    [Range(0f, 0.7f)]
    [SerializeField] private float stepSurgeStrength = 0.28f;

    [Tooltip("Strength of the speed dip during the foot plant phase (e.g. 0.22 = -22% speed lull).")]
    [Range(0f, 0.7f)]
    [SerializeField] private float stepDipStrength = 0.22f;

    [Header("Wipeout Fall & Skid")]
    [Tooltip("Damage taken when wiping out at max speed.")]
    [SerializeField] private int fallDamage = 1;

    [Tooltip("Duration of the skid after tipping over.")]
    [SerializeField] private float fallSkidDuration = 0.6f;

    [Tooltip("Skid velocity multiplier at start of fall.")]
    [SerializeField] private float fallSkidMultiplier = 1.1f;

    [Tooltip("Duration after skidding stops where the character is completely immobile and unable to move.")]
    [SerializeField] private float postSkidImmobileDuration = 1.0f;

    [Tooltip("Transform of the sprite (kept upright).")]
    [SerializeField] private Transform spriteTransform;

    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.25f;
    [SerializeField] private float staminaDrainPerSecond = 25f;
    [SerializeField] private float staminaRegenPerSecond = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;

    [Header("Distance Tracking")]
    [SerializeField] private float metresPerUnityUnit = 10f;
    [SerializeField] private float maxMovementPerPhysicsStep = 1f;

    [Header("Wall Collision")]
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private float wallBumpCooldown = 0.5f;
    [SerializeField] private float wallBumpDirectionThreshold = 0.5f;

    private Rigidbody2D rb;
    private PlayerStun playerStun;
    private PlayerSlip playerSlip;
    private PlayerHealth playerHealth;
    private PlayerSFX playerSFX;

    private Vector2 movement;
    private Vector2 previousPosition;
    private RunData runData;
    private bool sprintHeld;
    private float lastSprintTime = float.NegativeInfinity;

    private Vector2 currentVelocity;
    private float continuousMoveTimer;
    private bool isFallingSkid;
    private float fallSkidTimer;
    private Vector2 skidVelocity;
    private float nextWallBumpTime;
    private bool movementLocked;

    // Post-Skid Recovery State
    private float postSkidTimer;

    // Top Speed Sustain State
    private float currentTopSpeedTimer;

    // Inconsistent Sway State Machine
    private float currentSwayPulseTimer;
    private float currentSwayPulseDuration = 0.6f;
    private int currentSwayDirection = 1; // -1 = Left, +1 = Right
    private int consecutiveSwaysInCurrentDirection = 1;

    // Step-by-Step Cadence State Machine
    private float currentStepTimer;
    private float currentStepInterval = 0.42f;
    private float timeSinceLastMoveInput;

    // Counter-Braking State
    private bool isCounterBraking;

    public bool IsSprinting { get; private set; }
    public bool IsFallingSkid => isFallingSkid;
    public float ContinuousMoveTimer => continuousMoveTimer;
    public float TimeToMaxSpeed
    {
        get => timeToMaxSpeed;
        set => timeToMaxSpeed = Mathf.Max(0.5f, value);
    }
    public float BaseMoveSpeed => moveSpeed;
    public float MaxSpeedMultiplier => maxSpeedMultiplier;
    public bool IsMovementLocked => movementLocked;

    public float MasterSpeedScale
    {
        get => masterSpeedScale;
        set => masterSpeedScale = Mathf.Clamp(value, 0.1f, 5.0f);
    }

    public float Section1Acceleration
    {
        get => section1Acceleration;
        set => section1Acceleration = Mathf.Max(0.1f, value);
    }

    public float Section2Acceleration
    {
        get => section2Acceleration;
        set => section2Acceleration = Mathf.Max(0.1f, value);
    }

    public float LowSpeedTurnRate
    {
        get => lowSpeedTurnRate;
        set => lowSpeedTurnRate = Mathf.Max(10f, value);
    }

    public float HighSpeedTurnRate
    {
        get => highSpeedTurnRate;
        set => highSpeedTurnRate = Mathf.Max(10f, value);
    }

    public float PostSkidImmobileDuration
    {
        get => postSkidImmobileDuration;
        set => postSkidImmobileDuration = Mathf.Max(0f, value);
    }

    public float PostSkidTimer => postSkidTimer;
    public bool IsPostSkidImmobile => postSkidTimer > 0f;

    public float MaxTopSpeedDuration
    {
        get => maxTopSpeedDuration;
        set => maxTopSpeedDuration = Mathf.Max(0.1f, value);
    }

    public float CurrentTopSpeedTimer => currentTopSpeedTimer;
    public bool IsAtTopSpeed => currentTopSpeedTimer > 0f;
    public float TopSpeedTimeRemaining => Mathf.Max(0f, maxTopSpeedDuration - currentTopSpeedTimer);

    // Two-Phase Lean System Properties
    public float TransitionLeanThreshold
    {
        get => transitionLeanThreshold;
        set => transitionLeanThreshold = Mathf.Clamp(value, 0.05f, 0.95f);
    }

    public float LeanProgress =>
        Mathf.Clamp01(continuousMoveTimer / Mathf.Max(0.1f, timeToMaxSpeed));

    public bool IsInFirstSection => LeanProgress < transitionLeanThreshold;

    public int CurrentSwayDirection => currentSwayDirection;
    public int ConsecutiveSwaysInCurrentDirection => consecutiveSwaysInCurrentDirection;

    public float Section1SwayStrength
    {
        get => section1SwayStrength;
        set => section1SwayStrength = Mathf.Max(0f, value);
    }

    public float Section2SwayStrength
    {
        get => section2SwayStrength;
        set => section2SwayStrength = Mathf.Max(0f, value);
    }

    public bool EnableSection2Sway
    {
        get => enableSection2Sway;
        set => enableSection2Sway = value;
    }

    public Vector2 MovementInput => movement;
    public bool IsCounterBraking => isCounterBraking;
    public Vector2 CurrentVelocity => currentVelocity;

    // Step-by-Step Cadence Properties
    public bool EnableStepPulse
    {
        get => enableStepPulse;
        set => enableStepPulse = value;
    }

    public float StepDuration
    {
        get => stepDuration;
        set => stepDuration = Mathf.Max(0.1f, value);
    }

    public float StepSurgeStrength
    {
        get => stepSurgeStrength;
        set => stepSurgeStrength = Mathf.Clamp(value, 0f, 0.8f);
    }

    public float StepDipStrength
    {
        get => stepDipStrength;
        set => stepDipStrength = Mathf.Clamp(value, 0f, 0.8f);
    }

    public float CurrentStepCycleProgress =>
        Mathf.Clamp01(currentStepTimer / Mathf.Max(0.01f, currentStepInterval));

    // Backward-compatibility properties
    public float MaxTippingAngle => 28f;
    public bool IsMicroStumbling => false;
    public bool IsWallStumbling => false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStun = GetComponent<PlayerStun>();
        playerSlip = GetComponent<PlayerSlip>();
        playerSFX = GetComponent<PlayerSFX>();
        playerHealth = GetComponent<PlayerHealth>();

        currentSwayPulseDuration = Random.Range(minSwayPulseDuration, maxSwayPulseDuration);
        currentSwayDirection = Random.value < 0.5f ? -1 : 1;
        consecutiveSwaysInCurrentDirection = 1;
        currentStepInterval = stepDuration;

        if (GetComponent<PhoneTiltInput>() == null)
        {
            gameObject.AddComponent<PhoneTiltInput>();
        }

        if (spriteTransform == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spriteTransform = sr.transform;
        }

        if (spriteTransform != null)
        {
            spriteTransform.localRotation = Quaternion.identity;
        }
    }

    public void ClearFallAndRecovery()
    {
        isFallingSkid = false;
        isCounterBraking = false;
        postSkidTimer = 0f;
        currentTopSpeedTimer = 0f;
        continuousMoveTimer = 0f;
        currentVelocity = Vector2.zero;
        currentSwayPulseTimer = 0f;
        consecutiveSwaysInCurrentDirection = 1;

        if (spriteTransform != null)
        {
            spriteTransform.localRotation = Quaternion.identity;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private Vector2 externalInputMovement;
    private bool externalInputSprint;
    private bool hasExternalInput;

    public bool HasExternalInput => hasExternalInput;
    public Vector2 ExternalInputMovement => externalInputMovement;

    /// <summary>
    /// Allows external input sources (such as a mobile phone gyroscope/accelerometer)
    /// to directly provide movement and sprint vectors.
    /// </summary>
    public void SetExternalInput(Vector2 inputMovement, bool isSprinting = false)
    {
        externalInputMovement = Vector2.ClampMagnitude(inputMovement, 1f);
        externalInputSprint = isSprinting;
        hasExternalInput = true;
    }

    public void ClearExternalInput()
    {
        hasExternalInput = false;
        externalInputMovement = Vector2.zero;
        externalInputSprint = false;
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            movement = Vector2.zero;
            sprintHeld = false;
            IsSprinting = false;
            currentVelocity = Vector2.zero;
            continuousMoveTimer = 0f;
            currentTopSpeedTimer = 0f;
            isFallingSkid = false;
            postSkidTimer = 0f;
            currentSwayPulseTimer = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
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
        Vector2 kbMovement = Vector2.zero;
        bool kbSprint = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
                kbMovement.y += 1f;

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
                kbMovement.y -= 1f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                kbMovement.x -= 1f;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                kbMovement.x += 1f;

            kbSprint = Keyboard.current.leftShiftKey.isPressed;
        }

        // HYBRID CONTROL: Keyboard or Phone Tilt
        if (kbMovement.sqrMagnitude > 0.01f)
        {
            // Active keyboard input takes priority
            movement = kbMovement.normalized;
            sprintHeld = kbSprint;
        }
        else if (hasExternalInput && externalInputMovement.sqrMagnitude > 0.01f)
        {
            // Phone tilt drives when keyboard is idle
            movement = externalInputMovement;
            sprintHeld = externalInputSprint;
        }
        else
        {
            movement = Vector2.zero;
            sprintHeld = false;
        }

        UpdateCounterBrakingState();
    }

    private void UpdateCounterBrakingState()
    {
        Vector2 vel = currentVelocity.sqrMagnitude > 0.0001f
            ? currentVelocity
            : (rb != null ? rb.linearVelocity : Vector2.zero);

        float speed = vel.magnitude;

        if (movement.sqrMagnitude > 0.01f && speed > 0.01f)
        {
            float dot = Vector2.Dot(movement.normalized, vel / speed);
            isCounterBraking = dot < -0.15f;
        }
        else
        {
            isCounterBraking = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((wallLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        if (movement.sqrMagnitude < 0.01f && currentVelocity.sqrMagnitude < 0.1f)
            return;

        if (playerSlip != null && playerSlip.IsSlipping)
            return;

        if (playerStun != null && playerStun.IsStunned)
            return;

        if (isFallingSkid)
            return;

        if (Time.time < nextWallBumpTime)
            return;

        Vector2 checkDir = movement.sqrMagnitude > 0.01f ? movement : currentVelocity.normalized;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            float pushAmount = Vector2.Dot(checkDir, -contact.normal);
            if (pushAmount < wallBumpDirectionThreshold)
                continue;

            if (playerSFX != null)
                playerSFX.PlayWallBump();

            // Wall bump disrupts sustained unicycle momentum
            continuousMoveTimer = 0f;
            nextWallBumpTime = Time.time + wallBumpCooldown;
            break;
        }
    }

    private void FixedUpdate()
    {
        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (movementLocked)
        {
            movement = Vector2.zero;
            sprintHeld = false;
            IsSprinting = false;
            isCounterBraking = false;
            currentVelocity = Vector2.zero;
            continuousMoveTimer = 0f;
            isFallingSkid = false;

            rb.linearVelocity = Vector2.zero;
            previousPosition = rb.position;
            return;
        }

        bool runActive = IsRunActive();

        // Distance stat tracking
        Vector2 currentPosition = rb.position;
        float movedUnits = Vector2.Distance(previousPosition, currentPosition);
        previousPosition = currentPosition;

        if (runData != null && runActive &&
            (movement.sqrMagnitude > 0f || rb.linearVelocity.sqrMagnitude > 0.01f) &&
            movedUnits > 0f && movedUnits <= maxMovementPerPhysicsStep)
        {
            runData.AddDistance(movedUnits * metresPerUnityUnit);
        }

        // Active wipeout skid state
        if (isFallingSkid)
        {
            isCounterBraking = false;
            IsSprinting = false;
            RegenerateStamina(runActive);

            fallSkidTimer -= Time.fixedDeltaTime;

            float skidDecel = (fallSkidMultiplier * (moveSpeed + 2f) * masterSpeedScale) / Mathf.Max(0.01f, fallSkidDuration);
            skidVelocity = Vector2.MoveTowards(
                skidVelocity,
                Vector2.zero,
                skidDecel * Time.fixedDeltaTime
            );

            rb.linearVelocity = skidVelocity;

            if (fallSkidTimer <= 0f || skidVelocity.sqrMagnitude < 0.01f)
            {
                isFallingSkid = false;
                rb.linearVelocity = Vector2.zero;
                postSkidTimer = postSkidImmobileDuration;

                if (playerStun != null)
                    playerStun.ClearStun();
            }
            return;
        }

        // Post-skid immobile recovery period (player cannot move)
        if (postSkidTimer > 0f)
        {
            isCounterBraking = false;
            postSkidTimer -= Time.fixedDeltaTime;
            IsSprinting = false;
            RegenerateStamina(runActive);
            currentVelocity = Vector2.zero;
            continuousMoveTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Puddle slip override
        if (playerSlip != null && playerSlip.IsSlipping)
        {
            isCounterBraking = false;
            IsSprinting = false;
            RegenerateStamina(runActive);
            currentVelocity = playerSlip.SlipVelocity;
            continuousMoveTimer = 0f;
            rb.linearVelocity = playerSlip.SlipVelocity;
            return;
        }

        // Recovery stun override
        if (playerStun != null && playerStun.IsStunned)
        {
            isCounterBraking = false;
            IsSprinting = false;
            RegenerateStamina(runActive);
            currentVelocity = Vector2.zero;
            continuousMoveTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calculate unicycle base & runaway speed
        int speedLevel = runData != null
            ? runData.GetDistanceLevel(DistanceUpgrade.Speed)
            : 0;

        float currentBaseSpeed = (moveSpeed + speedLevel * speedBonusPerLevel) * masterSpeedScale;
        float currentMaxSpeed = currentBaseSpeed * maxSpeedMultiplier;

        // Sprinting
        bool hasSprintCapability = runData == null || (runData.SprintUnlocked && runData.CurrentSprintStamina > 0f);
        IsSprinting = sprintHeld && movement.sqrMagnitude > 0f && hasSprintCapability;

        if (IsSprinting && runData != null)
        {
            float staminaUsed = Mathf.Min(
                staminaDrainPerSecond * Time.fixedDeltaTime,
                runData.CurrentSprintStamina
            );
            runData.ChangeSprintStamina(-staminaUsed);
            lastSprintTime = Time.time;
        }
        else
        {
            RegenerateStamina(runActive);
        }

        // =======================================================
        // UNICYCLE PHYSICS ENGINE
        // =======================================================

        if (movement.sqrMagnitude > 0.01f)
        {
            float speed = currentVelocity.magnitude;
            bool isOpposingMotion = speed > 0.001f && Vector2.Dot(movement, currentVelocity / speed) < -0.15f;

            if (isOpposingMotion)
            {
                // OPPOSITE DIRECTION BRAKING:
                // When moving one direction and pushing the opposite way,
                // you MUST brake existing velocity down to zero before reversing!
                isCounterBraking = true;
                continuousMoveTimer = 0f;
                currentTopSpeedTimer = 0f;
                currentSwayPulseTimer = 0f;
                currentStepTimer = 0f;
                timeSinceLastMoveInput = 0f;

                currentVelocity = Vector2.MoveTowards(
                    currentVelocity,
                    Vector2.zero,
                    brakingDeceleration * masterSpeedScale * Time.fixedDeltaTime
                );

                if (currentVelocity.sqrMagnitude < 0.0001f)
                {
                    currentVelocity = Vector2.zero;
                }

                rb.linearVelocity = currentVelocity;
                return;
            }

            isCounterBraking = false;
            // FORWARD / STEERING CONE:
            // Continuously accelerating increases speed exponentially
            float timerMultiplier = IsSprinting ? 1.4f : 1.0f;
                continuousMoveTimer += Time.fixedDeltaTime * timerMultiplier;

                float progress = LeanProgress;
                float expFactor = Mathf.Pow(progress, speedExponent);
                float currentTargetSpeed = Mathf.Lerp(currentBaseSpeed, currentMaxSpeed, expFactor);

                if (IsSprinting)
                    currentTargetSpeed *= sprintMultiplier;

                // Tipping point check: sustain max runaway speed for configurable seconds before falling
                if (progress >= 1f || currentTargetSpeed >= currentMaxSpeed * 0.98f)
                {
                    currentTopSpeedTimer += Time.fixedDeltaTime;

                    if (currentTopSpeedTimer >= maxTopSpeedDuration)
                    {
                        Vector2 fallDir = currentVelocity.sqrMagnitude > 0.1f
                            ? currentVelocity.normalized
                            : movement;

                        TriggerFallAndSkid(currentTargetSpeed, fallDir);
                        return;
                    }
                }
                else
                {
                    currentTopSpeedTimer = Mathf.Max(0f, currentTopSpeedTimer - Time.fixedDeltaTime * 2f);
                }

                // Two-Phase Organic Sway:
                Vector2 targetDir = movement;
                bool shouldSway = IsInFirstSection || enableSection2Sway;
                if (shouldSway)
                {
                // Inconsistent frequency sway with max 3 consecutive sways in one direction
                currentSwayPulseTimer += Time.fixedDeltaTime;
                if (currentSwayPulseTimer >= currentSwayPulseDuration)
                {
                    currentSwayPulseTimer = 0f;
                    currentSwayPulseDuration = Random.Range(minSwayPulseDuration, maxSwayPulseDuration);

                    // If 3 consecutive sways in the same direction, MUST switch to opposite direction
                    if (consecutiveSwaysInCurrentDirection >= maxConsecutiveSwaysSameDirection)
                    {
                        currentSwayDirection = -currentSwayDirection;
                        consecutiveSwaysInCurrentDirection = 1;
                    }
                    else
                    {
                        // 50% chance to switch or repeat same direction
                        bool switchDirection = Random.value < 0.5f;
                        if (switchDirection)
                        {
                            currentSwayDirection = -currentSwayDirection;
                            consecutiveSwaysInCurrentDirection = 1;
                        }
                        else
                        {
                            consecutiveSwaysInCurrentDirection++;
                        }
                    }
                }

                // Smooth half-sine pulse: 0 -> peak -> 0
                float pulseProgress = Mathf.Clamp01(currentSwayPulseTimer / Mathf.Max(0.01f, currentSwayPulseDuration));
                float pulseCurve = Mathf.Sin(pulseProgress * Mathf.PI);

                // Sway strength in Section 1 vs Section 2
                float currentSwayStrength = IsInFirstSection
                    ? section1SwayStrength
                    : section2SwayStrength;

                Vector2 perp = new Vector2(-movement.y, movement.x);
                Vector2 swayOffset = perp * (pulseCurve * currentSwayDirection * currentSwayStrength);

                    targetDir = (movement + swayOffset).normalized;
                }
                else
                {
                    // Section 2 without sway: straight runaway momentum (0 sway)
                    targetDir = movement;
                    currentSwayPulseTimer = 0f;
                }

                // Symmetrical acceleration:
                // Section 1 acceleration matches brakingDeceleration (same rate speeding up as slowing down)
                // Section 2 uses section2Acceleration for runaway responsiveness
                float currentAccel = (IsInFirstSection ? section1Acceleration : section2Acceleration) * masterSpeedScale;

                // Speed-dependent angular steering:
                // At low speeds: tight turn rate (sharp, responsive redirection).
                // At high speeds: turn rate decreases, producing a wide sweeping curve when turning 90°.
                float currentSpeed = currentVelocity.magnitude;
                float speedRatio = Mathf.Clamp01(currentSpeed / Mathf.Max(0.1f, currentMaxSpeed));
                float currentTurnRateDeg = Mathf.Lerp(lowSpeedTurnRate, highSpeedTurnRate, speedRatio) * masterSpeedScale;

                Vector2 steeringHeading;
                if (currentSpeed > 0.08f)
                {
                    float maxRadiansDelta = currentTurnRateDeg * Mathf.Deg2Rad * Time.fixedDeltaTime;
                    steeringHeading = Vector3.RotateTowards(
                        currentVelocity.normalized,
                        targetDir,
                        maxRadiansDelta,
                        0f
                    );
                }
                else
                {
                    steeringHeading = targetDir;
                }

                float newSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    currentTargetSpeed,
                    currentAccel * Time.fixedDeltaTime
                );

                // Step-by-Step Cadence Modulation:
                // In Section 1, movement surges during foot push-off and dips during foot plant,
                // making each step feel discrete rather than a continuous fluid glide.
                // In Section 2, step modulation is removed so movement becomes completely fluid runaway momentum.
                float stepMultiplier = 1.0f;
                if (IsInFirstSection && enableStepPulse)
                {
                    float timerRate = (IsSprinting ? 1.35f : 1.0f) * masterSpeedScale;
                    currentStepTimer += Time.fixedDeltaTime * timerRate;

                    if (currentStepTimer >= currentStepInterval)
                    {
                        currentStepTimer = 0f;
                        float stepVariation = Random.Range(0.88f, 1.12f);
                        currentStepInterval = stepDuration * stepVariation;
                    }

                    float stepCycle = Mathf.Clamp01(currentStepTimer / Mathf.Max(0.01f, currentStepInterval));
                    float stepModulation = 0f;

                    // Push-off surge (first 40% of cycle)
                    if (stepCycle < 0.4f)
                    {
                        float pushPhase = stepCycle / 0.4f;
                        stepModulation = Mathf.Sin(pushPhase * Mathf.PI) * stepSurgeStrength;
                    }
                    // Foot plant weight transfer deceleration dip (next 50% of cycle)
                    else if (stepCycle < 0.9f)
                    {
                        float plantPhase = (stepCycle - 0.4f) / 0.5f;
                        stepModulation = -Mathf.Sin(plantPhase * Mathf.PI) * stepDipStrength;
                    }

                    // Smoothly fade out step-by-step pulse as lean approaches transitionLeanThreshold
                    float section1Ratio = progress / Mathf.Max(0.01f, transitionLeanThreshold);
                    float stepFade = Mathf.Clamp01(1.0f - section1Ratio);

                    stepMultiplier = 1.0f + (stepModulation * stepFade);
                }
                else
                {
                    // Section 2: Zero step pulse - 100% fluid runaway glide
                    currentStepTimer = 0f;
                    stepMultiplier = 1.0f;
                }

                timeSinceLastMoveInput = 0f;
                currentVelocity = steeringHeading.normalized * newSpeed;
                rb.linearVelocity = currentVelocity * stepMultiplier;
        }
        else
        {
            isCounterBraking = false;
            // COASTING (Keys released):
            // Glide forward with unicycle inertia
            timeSinceLastMoveInput += Time.fixedDeltaTime;
            currentSwayPulseTimer = 0f;
            currentTopSpeedTimer = Mathf.Max(0f, currentTopSpeedTimer - Time.fixedDeltaTime * 2f);

            // Only reset step cycle if stopped or coasting for a noticeable pause (>0.25s),
            // so quick button tapping doesn't restart at the initial surge peak every frame
            if (timeSinceLastMoveInput > 0.25f || currentVelocity.sqrMagnitude < 0.04f)
            {
                currentStepTimer = 0f;
            }

            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                Vector2.zero,
                coastingDeceleration * masterSpeedScale * Time.fixedDeltaTime
            );

            if (currentVelocity.sqrMagnitude < 0.0001f)
            {
                currentVelocity = Vector2.zero;
            }

            continuousMoveTimer = Mathf.Max(
                0f,
                continuousMoveTimer - Time.fixedDeltaTime * momentumDecayRate
            );

            rb.linearVelocity = currentVelocity;
        }
    }

    private void TriggerFallAndSkid(float entrySpeed, Vector2 direction)
    {
        if (isFallingSkid)
            return;

        isFallingSkid = true;
        fallSkidTimer = fallSkidDuration;

        Vector2 skidDir = direction.sqrMagnitude > 0.01f
            ? direction.normalized
            : Vector2.down;

        skidVelocity = skidDir * (entrySpeed * fallSkidMultiplier);
        currentVelocity = Vector2.zero;
        continuousMoveTimer = 0f;
        currentTopSpeedTimer = 0f;
        postSkidTimer = 0f;
        currentSwayPulseTimer = 0f;
        currentStepTimer = 0f;

        // Deal damage from unicycle wipeout
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(fallDamage, DamageCause.Environmental);
        }

        // Play wipeout skid sound
        if (playerSFX != null)
        {
            playerSFX.PlaySlip();
        }

        Debug.Log(
            $"[UNICYCLE] Player wiped out at max speed {entrySpeed:F1} m/s! " +
            $"Skidding for {fallSkidDuration}s."
        );
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
