
using UnityEngine;

public class PlayerStun : MonoBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float baseStunDuration = 1.5f;
    [SerializeField] private float reductionPerLevel = 0.3f;
    [SerializeField] private float minimumStunDuration = 0.3f;

    private RunData runData;
    private float stunTimer;

    public bool IsStunned => stunTimer > 0f;

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;
    }

    private void Update()
    {
        if (stunTimer > 0f)
            stunTimer = Mathf.Max(
                0f,
                stunTimer - Time.deltaTime
            );
    }

    public void ApplyStun()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
            return;

        if (runData == null)
            runData = RunManager.Instance.Data;

        int recoveryLevel = 0;

        if (runData != null)
        {
            recoveryLevel = runData.GetDistanceLevel(
                DistanceUpgrade.Recovery
            );
        }

        float duration = Mathf.Max(
            minimumStunDuration,
            baseStunDuration -
            recoveryLevel * reductionPerLevel
        );

        stunTimer = Mathf.Max(stunTimer, duration);

        Debug.Log(
            $"Player stunned for {duration:F1}s " +
            $"(Recovery level {recoveryLevel})"
        );
    }

    public void ClearStun()
    {
        stunTimer = 0f;
    }
}
