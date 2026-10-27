
using UnityEngine;

public class PlayerDistanceTracker : MonoBehaviour
{
    [Header("Distance Conversion")]
    [Tooltip("How many metres equal one Unity unit.")]
    [SerializeField] private float metresPerUnityUnit = 10f;

    [Header("Movement Filtering")]
    [Tooltip("Ignore unusually large position jumps, such as teleports.")]
    [SerializeField] private float maxMovementPerFrame = 1f;

    [Header("Tracking")]
    [SerializeField] private bool trackOnlyDuringRun = true;

    private Vector3 previousPosition;
    private RunData runData;

    private void Start()
    {
        previousPosition = transform.position;

        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;
    }

    private void LateUpdate()
    {
        Vector3 currentPosition = transform.position;

        Vector2 movement = new Vector2(
            currentPosition.x - previousPosition.x,
            currentPosition.y - previousPosition.y
        );

        previousPosition = currentPosition;

        if (trackOnlyDuringRun &&
            (RunManager.Instance == null ||
             !RunManager.Instance.IsRunActive))
            return;

        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (runData == null)
            return;

        float movedUnits = movement.magnitude;

        if (movedUnits <= 0f ||
            movedUnits > maxMovementPerFrame)
            return;

        float metresEarned =
            movedUnits * metresPerUnityUnit;

        runData.AddDistance(metresEarned);
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
    }
}
