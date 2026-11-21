using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PhoneTiltInput : MonoBehaviour
{
    [Header("Tilt Sensitivity & Deadzones")]
    [Tooltip("Tilt angle (degrees) for maximum left/right steering.")]
    [SerializeField] private float maxSteerAngle = 35f;

    [Tooltip("Forward tilt angle (degrees) for maximum forward acceleration.")]
    [SerializeField] private float maxForwardAngle = 30f;

    [Tooltip("Backward tilt angle (degrees) for counter-steer braking.")]
    [SerializeField] private float maxBrakingAngle = 25f;

    [Tooltip("Deadzone in degrees to prevent hand tremor jitter.")]
    [SerializeField] private float deadzoneAngle = 3.5f;

    [Header("Sprint")]
    [Tooltip("Steep forward tilt angle that triggers sprinting.")]
    [SerializeField] private float sprintTiltAngle = 22f;

    [Header("Calibration")]
    [Tooltip("Automatically calibrate the resting holding angle when started.")]
    [SerializeField] private bool autoCalibrateOnStart = true;

    private PlayerController playerController;
    private Vector3 calibratedNeutralGravity = Vector3.down;
    private bool hasCalibrated = false;
    private Vector2 currentTiltVector = Vector2.zero;
    private bool currentTiltSprint = false;

    public Vector2 CurrentTiltVector => currentTiltVector;
    public bool CurrentTiltSprint => currentTiltSprint;
    public bool HasDeviceSensor { get; private set; }
    public bool IsCalibrated => hasCalibrated;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        EnableSensors();
    }

    private void Start()
    {
        EnableSensors();
        if (autoCalibrateOnStart)
        {
            CalibrateNeutral();
        }
    }

    private void OnEnable()
    {
        EnableSensors();
    }

    private void EnableSensors()
    {
        if (GravitySensor.current != null)
        {
            InputSystem.EnableDevice(GravitySensor.current);
            HasDeviceSensor = true;
        }
        else if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(Accelerometer.current);
            HasDeviceSensor = true;
        }
        else if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            HasDeviceSensor = true;
        }
    }

    /// <summary>
    /// Calibrates the natural resting hand angle (e.g. holding phone at a comfortable ~30 degree incline).
    /// </summary>
    public void CalibrateNeutral()
    {
        Vector3 raw = ReadRawGravity();
        if (raw.sqrMagnitude > 0.1f)
        {
            calibratedNeutralGravity = raw.normalized;
            hasCalibrated = true;
            Debug.Log($"[PHONE TILT] Calibrated neutral resting angle to: {calibratedNeutralGravity}");
        }
    }

    private Vector3 ReadRawGravity()
    {
        if (GravitySensor.current != null)
        {
            HasDeviceSensor = true;
            return GravitySensor.current.gravity.ReadValue();
        }

        if (Accelerometer.current != null)
        {
            HasDeviceSensor = true;
            return Accelerometer.current.acceleration.ReadValue();
        }

        return Vector3.down;
    }

    private void Update()
    {
        if (!HasDeviceSensor)
        {
            EnableSensors();
        }

        // [C] key recalibrates neutral angle
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            CalibrateNeutral();
        }

        // Tap screen recalibrates neutral angle
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CalibrateNeutral();
        }

        Vector3 rawGravity = ReadRawGravity();

        // Deviation from calibrated neutral resting hold
        Vector3 delta = rawGravity - (hasCalibrated ? calibratedNeutralGravity : Vector3.down);

        // In portrait holding:
        // delta.x = Left/Right tilt (roll)
        // delta.y = Forward/Backward tilt (pitch)
        float steerDeg = delta.x * 90f;
        float throttleDeg = delta.y * 90f;

        // Apply deadzone and normalize steering (-1 to 1)
        float steer = 0f;
        if (Mathf.Abs(steerDeg) > deadzoneAngle)
        {
            float sign = Mathf.Sign(steerDeg);
            steer = sign * Mathf.Clamp01(Mathf.InverseLerp(deadzoneAngle, maxSteerAngle, Mathf.Abs(steerDeg)));
        }

        // Apply deadzone and normalize forward/backward throttle (-1 to 1)
        float throttle = 0f;
        if (throttleDeg > deadzoneAngle)
        {
            // Tilt forward -> Drive forward
            throttle = Mathf.Clamp01(Mathf.InverseLerp(deadzoneAngle, maxForwardAngle, throttleDeg));
        }
        else if (throttleDeg < -deadzoneAngle)
        {
            // Tilt backward -> Counter-steer brake to 0
            throttle = -Mathf.Clamp01(Mathf.InverseLerp(deadzoneAngle, maxBrakingAngle, -throttleDeg));
        }

        currentTiltVector = new Vector2(steer, throttle);
        currentTiltSprint = throttleDeg >= sprintTiltAngle;

        // Feed directly into PlayerController
        if (playerController != null)
        {
            playerController.SetExternalInput(currentTiltVector, currentTiltSprint);
        }
    }
}
