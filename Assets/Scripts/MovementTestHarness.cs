using UnityEngine;
using UnityEngine.InputSystem;

public class MovementTestHarness : MonoBehaviour
{
    [Header("Testing Configuration")]
    [SerializeField] private RunData testRunData;
    [SerializeField] private bool godMode = true;
    [SerializeField] private bool showHUD = true;
    [Range(0.1f, 3.0f)]
    [SerializeField] private float masterTimeScale = 1.0f;

    private PlayerController player;
    private Rigidbody2D playerRb;
    private PlayerHealth playerHealth;
    private PlayerStun playerStun;
    private PlayerSlip playerSlip;
    private PlayerAnimationController playerAnim;
    private TestArea8DirSpriteController dir8Renderer;

    private void Awake()
    {
        EnsureActiveRun();
    }

    private void Start()
    {
        FindPlayer();
        EnsureActiveRun();
        SetTimeScale(masterTimeScale);
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        if (dir8Renderer != null)
        {
            dir8Renderer.IsOverrideActive = false;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        if (dir8Renderer != null)
        {
            dir8Renderer.IsOverrideActive = false;
        }
    }

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            playerHealth = player.GetComponent<PlayerHealth>();
            playerStun = player.GetComponent<PlayerStun>();
            playerSlip = player.GetComponent<PlayerSlip>();
            playerAnim = player.GetComponent<PlayerAnimationController>();

            // Only in the test area: attach and activate 8-directional Walking & Rotation sprite system
            dir8Renderer = player.GetComponent<TestArea8DirSpriteController>();
            if (dir8Renderer == null)
            {
                dir8Renderer = player.gameObject.AddComponent<TestArea8DirSpriteController>();
            }
            dir8Renderer.IsOverrideActive = true;
        }
    }

    private void EnsureActiveRun()
    {
        RunManager rm = RunManager.Instance;
        if (rm == null)
        {
            rm = FindAnyObjectByType<RunManager>();
            if (rm == null)
            {
                GameObject rmObj = new GameObject("RunManager");
                rm = rmObj.AddComponent<RunManager>();
            }
        }

        if (rm.Data == null)
        {
            if (testRunData == null)
            {
                testRunData = Resources.Load<RunData>("RunData");
                if (testRunData == null)
                    testRunData = ScriptableObject.CreateInstance<RunData>();
            }
            rm.SetRunData(testRunData);
        }

        if (rm.Data != null)
        {
            rm.Data.AddCoins(9999);
            rm.Data.GrantDistanceCurrency(99999);
            rm.Data.BuyCoinUpgrade(CoinUpgrade.Sprint);
        }

        if (!rm.IsRunActive)
        {
            rm.BeginNewRun();
            Debug.Log("[TEST HARNESS] Auto-started active run for movement testing.");
        }
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        // God Mode keep hearts topped up
        if (godMode && RunManager.Instance != null && RunManager.Instance.Data != null)
        {
            if (RunManager.Instance.Data.CurrentHearts < 2)
            {
                RunManager.Instance.Data.ChangeHearts(RunManager.Instance.Data.MaxHearts);
            }
        }

        HandleHotkeys();
    }

    private void HandleHotkeys()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // [Tab] Toggle HUD
        if (kb.tabKey.wasPressedThisFrame)
        {
            showHUD = !showHUD;
        }

        // [R] Reset player
        if (kb.rKey.wasPressedThisFrame)
        {
            ResetPlayer();
        }

        // [G] Toggle God Mode
        if (kb.gKey.wasPressedThisFrame)
        {
            godMode = !godMode;
            Debug.Log($"[TEST HARNESS] God Mode: {(godMode ? "ENABLED" : "DISABLED")}");
        }

        // [P] Spawn Puddle
        if (kb.pKey.wasPressedThisFrame)
        {
            SpawnPuddle();
        }

        // [K] Decrease Post-Skid Immobile Duration (-0.2s)
        if (kb.kKey.wasPressedThisFrame && player != null)
        {
            player.PostSkidImmobileDuration = Mathf.Max(0f, player.PostSkidImmobileDuration - 0.2f);
            Debug.Log($"[TEST HARNESS] Post-Skid Immobile Duration: {player.PostSkidImmobileDuration:F1}s");
        }

        // [L] Increase Post-Skid Immobile Duration (+0.2s)
        if (kb.lKey.wasPressedThisFrame && player != null)
        {
            player.PostSkidImmobileDuration += 0.2f;
            Debug.Log($"[TEST HARNESS] Post-Skid Immobile Duration: {player.PostSkidImmobileDuration:F1}s");
        }

        // [T] Decrease Top Speed Hold Duration (-0.2s)
        if (kb.tKey.wasPressedThisFrame && player != null)
        {
            player.MaxTopSpeedDuration = Mathf.Max(0.2f, player.MaxTopSpeedDuration - 0.2f);
            Debug.Log($"[TEST HARNESS] Top Speed Safe Duration: {player.MaxTopSpeedDuration:F1}s");
        }

        // [Y] Increase Top Speed Hold Duration (+0.2s)
        if (kb.yKey.wasPressedThisFrame && player != null)
        {
            player.MaxTopSpeedDuration += 0.2f;
            Debug.Log($"[TEST HARNESS] Top Speed Safe Duration: {player.MaxTopSpeedDuration:F1}s");
        }

        // [U] Decrease Transition Lean Threshold (-5%)
        if (kb.uKey.wasPressedThisFrame && player != null)
        {
            player.TransitionLeanThreshold -= 0.05f;
            Debug.Log($"[TEST HARNESS] Transition Lean Threshold: {player.TransitionLeanThreshold * 100f:F0}%");
        }

        // [I] Increase Transition Lean Threshold (+5%)
        if (kb.iKey.wasPressedThisFrame && player != null)
        {
            player.TransitionLeanThreshold += 0.05f;
            Debug.Log($"[TEST HARNESS] Transition Lean Threshold: {player.TransitionLeanThreshold * 100f:F0}%");
        }

        // [-] Decrease Time to 100% Lean (-0.5s)
        if (kb.minusKey.wasPressedThisFrame && player != null)
        {
            player.TimeToMaxSpeed = Mathf.Max(1.0f, player.TimeToMaxSpeed - 0.5f);
            Debug.Log($"[TEST HARNESS] Time to 100% Lean: {player.TimeToMaxSpeed:F1}s");
        }

        // [=] Increase Time to 100% Lean (+0.5s)
        if (kb.equalsKey.wasPressedThisFrame && player != null)
        {
            player.TimeToMaxSpeed += 0.5f;
            Debug.Log($"[TEST HARNESS] Time to 100% Lean: {player.TimeToMaxSpeed:F1}s");
        }

        // [[] Decrease Master Speed (-0.1x)
        if (kb.leftBracketKey.wasPressedThisFrame)
        {
            SetTimeScale(masterTimeScale - 0.1f);
        }

        // []] Increase Master Speed (+0.1x)
        if (kb.rightBracketKey.wasPressedThisFrame)
        {
            SetTimeScale(masterTimeScale + 0.1f);
        }

        // [\] Reset Master Speed (1.0x)
        if (kb.backslashKey.wasPressedThisFrame)
        {
            SetTimeScale(1.0f);
            if (player != null) player.MasterSpeedScale = 1.0f;
        }

        // [O] Toggle Step-by-Step Cadence Pulse
        if (kb.oKey.wasPressedThisFrame && player != null)
        {
            player.EnableStepPulse = !player.EnableStepPulse;
            Debug.Log($"[TEST HARNESS] Step-by-Step Pulse: {(player.EnableStepPulse ? "ENABLED (Step-by-Step)" : "DISABLED (Fluid)")}");
        }

        // [V] Toggle 8-Directional Walking & Rotation Sprites
        if (kb.vKey.wasPressedThisFrame && dir8Renderer != null)
        {
            dir8Renderer.IsOverrideActive = !dir8Renderer.IsOverrideActive;
            Debug.Log($"[TEST HARNESS] 8-Directional Sprites: {(dir8Renderer.IsOverrideActive ? "ACTIVE" : "ORIGINAL ANIMATOR")}");
        }

        // [H] Toggle Section 2 Sway
        if (kb.hKey.wasPressedThisFrame && player != null)
        {
            player.EnableSection2Sway = !player.EnableSection2Sway;
            Debug.Log($"[TEST HARNESS] Section 2 Sway: {(player.EnableSection2Sway ? "ENABLED (Swaying in Sec 2)" : "DISABLED (Straight Rush)")}");
        }

        // [1-5] Balance Upgrades
        if (RunManager.Instance != null && RunManager.Instance.Data != null)
        {
            RunData data = RunManager.Instance.Data;
            data.GrantDistanceCurrency(99999);

            if (kb.digit1Key.wasPressedThisFrame) SetBalance(0);
            if (kb.digit2Key.wasPressedThisFrame) SetBalance(1);
            if (kb.digit3Key.wasPressedThisFrame) SetBalance(2);
            if (kb.digit4Key.wasPressedThisFrame) SetBalance(3);
            if (kb.digit5Key.wasPressedThisFrame) SetBalance(4);

            if (kb.digit6Key.wasPressedThisFrame) SetSpeed(0);
            if (kb.digit7Key.wasPressedThisFrame) SetSpeed(1);
            if (kb.digit8Key.wasPressedThisFrame) SetSpeed(2);
            if (kb.digit9Key.wasPressedThisFrame) SetSpeed(3);
            if (kb.digit0Key.wasPressedThisFrame) SetSpeed(4);
        }
    }

    private void SetBalance(int targetLevel)
    {
        if (RunManager.Instance == null || RunManager.Instance.Data == null) return;
        RunData data = RunManager.Instance.Data;
        while (data.GetDistanceLevel(DistanceUpgrade.Balance) < targetLevel)
        {
            data.BuyDistanceUpgrade(DistanceUpgrade.Balance);
        }
        Debug.Log($"[TEST HARNESS] Balance level set to: {data.GetDistanceLevel(DistanceUpgrade.Balance)}");
    }

    private void SetSpeed(int targetLevel)
    {
        if (RunManager.Instance == null || RunManager.Instance.Data == null) return;
        RunData data = RunManager.Instance.Data;
        while (data.GetDistanceLevel(DistanceUpgrade.Speed) < targetLevel)
        {
            data.BuyDistanceUpgrade(DistanceUpgrade.Speed);
        }
        Debug.Log($"[TEST HARNESS] Speed level set to: {data.GetDistanceLevel(DistanceUpgrade.Speed)}");
    }

    private void ResetPlayer()
    {
        if (player == null) return;

        player.transform.position = Vector3.zero;
        player.ClearFallAndRecovery();

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        if (playerStun != null)
            playerStun.ClearStun();

        if (playerSlip != null)
            playerSlip.ClearSlip();

        if (RunManager.Instance != null && RunManager.Instance.Data != null)
        {
            RunManager.Instance.Data.ChangeHearts(RunManager.Instance.Data.MaxHearts);
        }

        Debug.Log("[TEST HARNESS] Player reset to origin (0, 0).");
    }

    private void SpawnPuddle()
    {
        Vector3 spawnPos = player != null && playerRb != null && playerRb.linearVelocity.sqrMagnitude > 0.1f
            ? player.transform.position + (Vector3)playerRb.linearVelocity.normalized * 2f
            : (player != null ? player.transform.position + Vector3.right * 1.5f : Vector3.zero);

        PuddleHazard existing = FindAnyObjectByType<PuddleHazard>();
        if (existing != null)
        {
            GameObject newPuddle = Instantiate(existing.gameObject, spawnPos, Quaternion.identity);
            newPuddle.name = "Puddle_Dynamic";
            Debug.Log($"[TEST HARNESS] Spawned Puddle at {spawnPos}");
        }
        else
        {
            Debug.LogWarning("[TEST HARNESS] No puddle found in scene to clone.");
        }
    }

    public void SetTimeScale(float scale)
    {
        masterTimeScale = Mathf.Clamp(scale, 0.1f, 3.0f);
        Time.timeScale = masterTimeScale;
        Time.fixedDeltaTime = 0.02f * masterTimeScale;
        Debug.Log($"[TEST HARNESS] Master Speed Scale: {masterTimeScale:F2}x (Physics fixedDeltaTime: {Time.fixedDeltaTime:F4}s)");
    }

    private void OnGUI()
    {
        if (!showHUD) return;

        // Auto-scale GUI for high-res displays (e.g. 1440p / 4K UHD)
        float baseHeight = 1080f;
        float scale = Mathf.Max(1f, Screen.height / baseHeight);
        Matrix4x4 origMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float width = 480f;
        float height = 805f;
        Rect boxRect = new Rect(15f, 15f, width, height);

        GUI.Box(boxRect, "=== UNICYCLE 2-PHASE MOVEMENT GYM ===");

        GUILayout.BeginArea(new Rect(25f, 35f, width - 20f, height - 40f));

        if (playerRb != null)
        {
            float speed = playerRb.linearVelocity.magnitude;
            GUILayout.Label($"Velocity: {speed:F2} m/s");
        }

        // --- MASTER VOLUME SPEED CONTROL ---
        GUILayout.Space(2f);
        GUI.color = new Color(1f, 0.85f, 0.2f);
        GUILayout.Label($"<b>MASTER SPEED (Game Clock): {masterTimeScale:F2}x</b> [Keys: '[' / ']']");
        GUI.color = Color.white;

        float newTimeScale = GUILayout.HorizontalSlider(masterTimeScale, 0.1f, 2.5f);
        if (Mathf.Abs(newTimeScale - masterTimeScale) > 0.005f)
        {
            SetTimeScale(newTimeScale);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("0.25x (Slow-Mo)")) SetTimeScale(0.25f);
        if (GUILayout.Button("0.5x (Half)")) SetTimeScale(0.5f);
        if (GUILayout.Button("1.0x (Normal)")) SetTimeScale(1.0f);
        if (GUILayout.Button("1.5x (Fast)")) SetTimeScale(1.5f);
        if (GUILayout.Button("2.0x (Turbo)")) SetTimeScale(2.0f);
        GUILayout.EndHorizontal();

        if (player != null)
        {
            GUILayout.Space(2f);
            GUILayout.Label($"Player Movement Speed Scale: <b>{player.MasterSpeedScale:F2}x</b>");
            float newPlayerScale = GUILayout.HorizontalSlider(player.MasterSpeedScale, 0.2f, 2.5f);
            if (Mathf.Abs(newPlayerScale - player.MasterSpeedScale) > 0.005f)
            {
                player.MasterSpeedScale = newPlayerScale;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x Player")) player.MasterSpeedScale = 0.5f;
            if (GUILayout.Button("1.0x Default")) player.MasterSpeedScale = 1.0f;
            if (GUILayout.Button("1.5x Player")) player.MasterSpeedScale = 1.5f;
            if (GUILayout.Button("2.0x Player")) player.MasterSpeedScale = 2.0f;
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(4f);

        if (player != null)
        {
            // Input source telemetry
            string inputSource = "KEYBOARD (WASD)";
            if (player.HasExternalInput && player.ExternalInputMovement.sqrMagnitude > 0.01f)
            {
                inputSource = $"PHONE TILT (Steer: {player.ExternalInputMovement.x:+0.00;-0.00}, Throttle: {player.ExternalInputMovement.y:+0.00;-0.00})";
            }
            GUILayout.Label($"Active Input: <color=#00FF88><b>{inputSource}</b></color>");

            PhoneTiltInput phoneInput = player.GetComponent<PhoneTiltInput>();
            bool hasSensor = phoneInput != null && phoneInput.HasDeviceSensor;
            string sensorStatus = hasSensor
                ? "<color=#55FF55>CONNECTED (STREAMING GYRO)</color>"
                : "<color=#FFAA33>NOT DETECTED (See Unity Remote Guide)</color>";
            GUILayout.Label($"Mobile Gyro Sensor: <b>{sensorStatus}</b>");

            float holdTime = player.ContinuousMoveTimer;
            float leanPct = player.LeanProgress * 100f;
            float threshPct = player.TransitionLeanThreshold * 100f;
            bool isSec1 = player.IsInFirstSection;

            // Section Banner
            string secBanner = isSec1
                ? "<color=#FFEE44><b>[SECTION 1: INCONSISTENT SWAY & STEPS]</b></color>"
                : "<color=#FF6644><b>[SECTION 2: STRAIGHT RUNAWAY RUSH]</b></color>";

            GUILayout.Label(secBanner);

            // 8-Directional Walking & Rotation Sprite Telemetry
            if (dir8Renderer != null)
            {
                string dir8Status = dir8Renderer.IsOverrideActive
                    ? $"<color=#55FF55>ACTIVE (8-DIR)</color> (Facing: <b>{dir8Renderer.CurrentDirectionName}</b>, Frame: {dir8Renderer.CurrentFrameIndex + 1}/8)"
                    : "<color=#FFAA33>ORIGINAL 4-DIR ANIMATOR</color>";
                GUILayout.Label($"Test Area Sprites: {dir8Status} [Key 'V']");
            }

            // Step-by-Step Cadence Telemetry
            string stepStatus = player.EnableStepPulse
                ? "<color=#55FF55>STEP-BY-STEP (Surge & Foot Plant)</color>"
                : "<color=#FFAA33>FLUID (Continuous Glide)</color>";
            GUILayout.Label($"Section 1 Locomotion: <b>{stepStatus}</b> [Key 'O' to Toggle]");

            // Animation Playback Telemetry
            if (playerAnim != null)
            {
                GUILayout.Label($"Animation Playback Speed: <color=#00FFFF><b>{playerAnim.CurrentAnimPlaybackSpeed:F2}x</b></color> (Sec 1: {playerAnim.Section1AnimSpeed:F1}x -> Sec 2 Max: {playerAnim.Section2MaxAnimSpeed:F1}x)");
            }

            // Lean meter & Threshold
            GUILayout.Label($"Lean: {leanPct:F0}% / 100% (Build Time: {player.TimeToMaxSpeed:F1}s, Keys [-]/[=])");
            GUILayout.Label($"Transition Threshold: {threshPct:F0}% (Keys [U]/[I] -/+5%)");

            // Top speed window & countdown
            GUILayout.Label($"Top Speed Safe Window: {player.MaxTopSpeedDuration:F1}s (Keys [T]/[Y] -/+0.2s)");
            if (player.IsAtTopSpeed)
            {
                GUILayout.Label($"<color=#FF2222><b>>>> AT TOP SPEED! FALL IN {player.TopSpeedTimeRemaining:F1}s <<<</b></color>");
            }

            // Sway side & streak
            string sideName = player.CurrentSwayDirection < 0 ? "LEFT" : "RIGHT";
            float curStr = isSec1 ? player.Section1SwayStrength : (player.EnableSection2Sway ? player.Section2SwayStrength : 0f);
            string secLabel = isSec1 ? "Sec 1" : (player.EnableSection2Sway ? "Sec 2 (Swaying)" : "Sec 2 (Straight)");
            GUILayout.Label($"Active Sway: <color=#00FFFF>{sideName}</color> (Streak: {player.ConsecutiveSwaysInCurrentDirection}/3 max) | Str: {curStr:F2} [{secLabel}]");

            string sec2SwayText = player.EnableSection2Sway
                ? "<color=#00FFFF>ENABLED (Swaying)</color>"
                : "<color=#888888>OFF (Straight Rush)</color>";
            GUILayout.Label($"Section 2 Sway: <b>{sec2SwayText}</b> [Key 'H' to Toggle]");

            // Post-skid recovery setting
            GUILayout.Label($"Post-Skid Immobile: {player.PostSkidImmobileDuration:F1}s (Keys [K]/[L] -/+0.2s)");

            // Turning arc rate
            float currentTurnRate = Mathf.Lerp(player.LowSpeedTurnRate, player.HighSpeedTurnRate, player.LeanProgress);
            GUILayout.Label($"Steering Turn Rate: {currentTurnRate:F0}°/s (90° Curve: {(90f / currentTurnRate):F2}s)");

            string status = "IDLE";
            if (player.IsFallingSkid) status = "WIPEOUT! FALLING & SKIDDING";
            else if (player.IsPostSkidImmobile) status = $"IMMOBILE AFTER SKID ({player.PostSkidTimer:F1}s remaining)";
            else if (playerStun != null && playerStun.IsStunned) status = "STUNNED / RECOVERING";
            else if (playerSlip != null && playerSlip.IsSlipping) status = "SLIPPING ON PUDDLE";
            else if (playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
            {
                if (player.IsCounterBraking)
                {
                    status = "<color=#FFFF22>BRAKING & CATCHING BALANCE (RAPID STEPS)</color>";
                }
                else if (player.IsSprinting)
                {
                    status = "SPRINTING (FAST ACCELERATION)";
                }
                else if (player.MovementInput.sqrMagnitude > 0.01f)
                {
                    status = isSec1 ? "DRIVING (ACCEL: 6.5 m/s² MATCHES BRAKING)" : "RUNAWAY DRIVE (STRAIGHT & LOCKED)";
                }
                else
                {
                    status = "COASTING (UNICYCLE GLIDE)";
                }
            }

            GUILayout.Label($"Current State: [<b>{status}</b>]");
        }

        if (RunManager.Instance != null && RunManager.Instance.Data != null)
        {
            RunData data = RunManager.Instance.Data;
            int bal = data.GetDistanceLevel(DistanceUpgrade.Balance);
            int spd = data.GetDistanceLevel(DistanceUpgrade.Speed);
            GUILayout.Label($"Hearts: {data.CurrentHearts} / {data.MaxHearts} | God Mode: {(godMode ? "ON" : "OFF")}");
            GUILayout.Label($"Balance Lvl: {bal} (Keys [1-5]) | Speed Lvl: {spd} (Keys [6-0])");
        }

        GUILayout.Space(4f);
        GUILayout.Label("--- TEST CONTROLS ---");
        GUILayout.Label("[WASD] Keyboard | [Tilt Phone] Phone Gyro | [C] Calibrate");
        GUILayout.Label("[Opposite Key / Tilt Back] Brake to 0 | [Shift] Sprint");
        GUILayout.Label("['[' / ']'] Master Speed -/+0.1x | [ \\ ] Reset to 1.0x");
        GUILayout.Label("[V] 8-Dir Sprites | [H] Sec 2 Sway | [O] Step Pulse | [-]/[=] Lean Time");
        GUILayout.Label("[U] / [I] Lean Threshold | [T] / [Y] Top Speed Window");
        GUILayout.Label("[K] / [L] Post-Skid Immobile | [R] Reset | [G] God Mode | [Tab] HUD");

        GUILayout.EndArea();

        GUI.matrix = origMatrix;
    }
}
