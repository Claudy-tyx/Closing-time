
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSlip : MonoBehaviour
{
    private Rigidbody2D rb;
    private RunData runData;

    private float slipTimer;
    private float slipSpeed;
    private Vector2 slipDirection;

    public bool IsSlipping => slipTimer > 0f;
    public Vector2 SlipVelocity => slipDirection * slipSpeed;
    private PlayerSFX playerSFX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSFX = GetComponent<PlayerSFX>();
    }

    private void Update()
    {
        if (slipTimer > 0f)
            slipTimer = Mathf.Max(
                0f,
                slipTimer - Time.deltaTime
            );
    }

    
    public void TrySlip(float duration, float speed)
    {
        if (IsSlipping)
        {
            Debug.Log("[SLIP] Already slipping.");
            return;
        }

        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
        {
            Debug.Log("[SLIP] Run is not active.");
            return;
        }

        if (runData == null)
            runData = RunManager.Instance.Data;

        if (runData == null)
        {
            Debug.LogWarning("[SLIP] RunData is missing!");
            return;
        }

        if (runData.NonSlipShoesUnlocked)
        {
            Debug.Log("[SLIP] Non-Slip Shoes prevent slipping.");
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        if (velocity.sqrMagnitude < 0.01f)
        {
            Debug.Log("[SLIP] Player velocity is too low.");
            return;
        }

        // Random slip direction within the backward-facing 180° arc.
        // 90° to 270° relative to the player's entry movement.
        float randomAngle = Random.Range(-90f, 90f);

        Vector2 entryDirection = velocity.normalized;

        float radians = randomAngle * Mathf.Deg2Rad;

        slipDirection = new Vector2(
            entryDirection.x * Mathf.Cos(radians) -
            entryDirection.y * Mathf.Sin(radians),

            entryDirection.x * Mathf.Sin(radians) +
            entryDirection.y * Mathf.Cos(radians)
        ).normalized;

        slipSpeed = speed;
        slipTimer = duration;
        if (playerSFX != null)
            playerSFX.PlaySlip();

        Debug.Log(
            $"[SLIP] Started! Direction: {slipDirection}, " +
            $"Speed: {slipSpeed}, Duration: {slipTimer}"
        );
    }


    public void ClearSlip()
    {
        slipTimer = 0f;
    }
}
