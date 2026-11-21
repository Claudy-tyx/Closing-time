using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestArea8DirSpriteController : MonoBehaviour
{
    public enum Direction8
    {
        East = 0,
        NorthEast = 1,
        North = 2,
        NorthWest = 3,
        West = 4,
        SouthWest = 5,
        South = 6,
        SouthEast = 7
    }

    [System.Serializable]
    public class DirectionData
    {
        public string name;
        public Sprite idleSprite;
        public Sprite[] walkFrames = new Sprite[8];
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator originalAnimator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Animation Settings")]
    [Tooltip("Base frames-per-second for walking in Section 1.")]
    [SerializeField] private float baseFps = 12f;

    [Tooltip("Maximum frames-per-second when at top runaway speed in Section 2.")]
    [SerializeField] private float maxRunawayFps = 24f;

    [Tooltip("Frames-per-second multiplier when sprinting.")]
    [SerializeField] private float sprintFpsMultiplier = 1.25f;

    [Tooltip("Frames-per-second when counter-braking (rapid catching steps to bleed momentum).")]
    [SerializeField] private float brakingQuickStepFps = 20f;

    [Header("8-Directional Sprite Sets")]
    [SerializeField] private DirectionData[] directions = new DirectionData[8];

    private Direction8 currentDirection = Direction8.South;
    private float animationTimer;
    private int currentFrameIndex;
    private bool isOverrideActive = true;
    private Sprite originalSpriteBeforeOverride;

    public bool IsOverrideActive
    {
        get => isOverrideActive;
        set
        {
            isOverrideActive = value;
            if (originalAnimator != null)
            {
                originalAnimator.enabled = !isOverrideActive;
            }
        }
    }

    public Direction8 CurrentFacingDirection => currentDirection;
    public int CurrentFrameIndex => currentFrameIndex;
    public string CurrentDirectionName => directions != null && (int)currentDirection < directions.Length && directions[(int)currentDirection] != null
        ? directions[(int)currentDirection].name
        : currentDirection.ToString();

    private void Awake()
    {
        ResolveReferences();
        InitializeSpritesIfNeeded();
    }

    private void Start()
    {
        ResolveReferences();
        InitializeSpritesIfNeeded();

        if (originalAnimator != null && isOverrideActive)
        {
            originalAnimator.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (originalAnimator != null && isOverrideActive)
        {
            originalAnimator.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (originalAnimator != null)
        {
            originalAnimator.enabled = true;
        }

        if (spriteRenderer != null && originalSpriteBeforeOverride != null)
        {
            spriteRenderer.sprite = originalSpriteBeforeOverride;
        }
    }

    private void ResolveReferences()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && originalSpriteBeforeOverride == null)
            {
                originalSpriteBeforeOverride = spriteRenderer.sprite;
            }
        }

        if (originalAnimator == null)
        {
            originalAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void InitializeSpritesIfNeeded()
    {
        if (IsFullyLoaded())
            return;

#if UNITY_EDITOR
        LoadSpritesFromProject();
#endif
    }

    public bool IsFullyLoaded()
    {
        if (directions == null || directions.Length != 8)
            return false;

        for (int i = 0; i < 8; i++)
        {
            if (directions[i] == null || directions[i].idleSprite == null)
                return false;

            if (directions[i].walkFrames == null || directions[i].walkFrames.Length < 8)
                return false;

            for (int f = 0; f < 8; f++)
            {
                if (directions[i].walkFrames[f] == null)
                    return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Reload Sprites From Project")]
    public void LoadSpritesFromProject()
    {
        string[] dirKeys = new string[] {
            "east", "north-east", "north", "north-west",
            "west", "south-west", "south", "south-east"
        };

        directions = new DirectionData[8];

        for (int i = 0; i < 8; i++)
        {
            string key = dirKeys[i];
            directions[i] = new DirectionData
            {
                name = key.ToUpperInvariant()
            };

            // 1. Idle Rotation Sprite from Assets/Art/Player/rotations/{key}.png
            string rotPath = $"Assets/Art/Player/rotations/{key}.png";
            directions[i].idleSprite = LoadSpriteAsset(rotPath);

            // 2. Walking Frames from Assets/Art/Player/Walking/{key}/frame_{f:D3}.png
            directions[i].walkFrames = new Sprite[8];
            for (int f = 0; f < 8; f++)
            {
                string walkPath = $"Assets/Art/Player/Walking/{key}/frame_{f:D3}.png";
                directions[i].walkFrames[f] = LoadSpriteAsset(walkPath);
            }
        }

        Debug.Log("[TEST AREA] 8-Directional Walking & Rotation sprites successfully loaded from Assets/Art/Player!");
    }

    private Sprite LoadSpriteAsset(string path)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                    return s;
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
#endif

    private void Update()
    {
        if (!isOverrideActive || spriteRenderer == null)
            return;

        if (originalAnimator != null && originalAnimator.enabled)
        {
            originalAnimator.enabled = false;
        }

        Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;
        Vector2 input = playerController != null ? playerController.MovementInput : Vector2.zero;

        bool hasInput = input.sqrMagnitude > 0.01f;
        float speedSqr = velocity.sqrMagnitude;
        bool hasVelocity = speedSqr > 0.04f;

        // Is moving: active if keys are held, or still coasting with speed
        bool isMoving = hasInput || hasVelocity;

        // Freeze animation when stunned, slipping, or immobile after skid
        PlayerStun stun = GetComponent<PlayerStun>();
        if (stun != null && stun.IsStunned) isMoving = false;

        PlayerSlip slip = GetComponent<PlayerSlip>();
        if (slip != null && slip.IsSlipping) isMoving = false;

        if (playerController != null && playerController.IsPostSkidImmobile) isMoving = false;

        // Reverse braking: active input clearly opposes velocity (speed > 0.03 units/s)
        bool isReverseBraking = hasInput && speedSqr > 0.001f && Vector2.Dot(input, velocity) < -0.05f;

        // Determine facing direction:
        // Use intentional movement input (WASD / Phone tilt) so that physical sway wobble
        // NEVER causes diagonal animations to trigger when moving forward, back, left, or right!
        if (playerController != null && playerController.IsFallingSkid && speedSqr > 0.05f)
        {
            // Wipeout skid: face the direction of the physical slide
            currentDirection = ResolveDirectionFromInput(velocity, currentDirection);
            isMoving = true;
        }
        else if (isReverseBraking)
        {
            // Compensating to slow down (reverse braking):
            // The character is moving in one direction and pressing the opposite way to brake!
            // Maintain current facing direction (do not flip!) until travel direction actually reverses.
            isMoving = true;
        }
        else if (hasInput)
        {
            // Active driving: face the intentional input direction (strictly immune to sway)
            currentDirection = ResolveDirectionFromInput(input, currentDirection);
        }
        // When coasting, preserve currentDirection without changing it

        DirectionData activeData = directions[(int)currentDirection];
        if (activeData == null)
            return;

        if (isMoving)
        {
            // Dynamic animation playback speed:
            float fps = baseFps;
            if (playerController != null)
            {
                if (isReverseBraking || playerController.IsCounterBraking)
                {
                    // Braking / catching themselves: rapid catching steps
                    fps = brakingQuickStepFps;
                }
                else if (!playerController.IsInFirstSection)
                {
                    float lean = playerController.LeanProgress;
                    float thresh = playerController.TransitionLeanThreshold;
                    float sec2Ratio = Mathf.Clamp01((lean - thresh) / Mathf.Max(0.01f, 1.0f - thresh));
                    fps = Mathf.Lerp(baseFps, maxRunawayFps, sec2Ratio);
                }

                if (playerController.IsSprinting)
                {
                    fps *= sprintFpsMultiplier;
                }
            }

            animationTimer += Time.deltaTime * fps;
            currentFrameIndex = ((int)animationTimer) % 8;

            if (activeData.walkFrames != null && activeData.walkFrames.Length == 8 && activeData.walkFrames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = activeData.walkFrames[currentFrameIndex];
            }
        }
        else
        {
            // Stopped / Idle: display the static rotation sprite for current facing direction
            animationTimer = 0f;
            currentFrameIndex = 0;

            if (activeData.idleSprite != null)
            {
                spriteRenderer.sprite = activeData.idleSprite;
            }
        }
    }

    public static Direction8 ResolveDirectionFromInput(Vector2 input, Direction8 fallback)
    {
        if (input.sqrMagnitude < 0.01f)
            return fallback;

        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        // Cardinal priority bias:
        // A diagonal is only selected if both X and Y inputs are clearly engaged (> 50% of each other).
        // If one axis dominates, snap strictly to North, South, East, or West.
        const float cardinalBiasRatio = 0.5f;

        if (absX < absY * cardinalBiasRatio)
        {
            return input.y > 0f ? Direction8.North : Direction8.South;
        }

        if (absY < absX * cardinalBiasRatio)
        {
            return input.x > 0f ? Direction8.East : Direction8.West;
        }

        // Both axes are substantially engaged -> intentional diagonal input
        if (input.x > 0f && input.y > 0f) return Direction8.NorthEast;
        if (input.x < 0f && input.y > 0f) return Direction8.NorthWest;
        if (input.x > 0f && input.y < 0f) return Direction8.SouthEast;
        return Direction8.SouthWest;
    }

    public static Direction8 VectorToDirection8(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f)
            return Direction8.South;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0f)
            angle += 360f;

        // 8 octants of 45° centered at:
        // 0° = East, 45° = NorthEast, 90° = North, 135° = NorthWest,
        // 180° = West, 225° = SouthWest, 270° = South, 315° = SouthEast
        int octant = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        return (Direction8)octant;
    }
}
