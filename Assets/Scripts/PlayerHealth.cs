
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Damage Protection")]
    [SerializeField] private float invulnerabilityTime = 1f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private bool flashWhenDamaged = true;
    [SerializeField] private float flashInterval = 0.1f;

    private PlayerStun playerStun;
    private RunData runData;
    private float invulnerabilityTimer;
    private float flashTimer;
    private bool spriteHidden;
    private Color originalColor;
    private PlayerSFX playerSFX;

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (playerSprite != null)
            originalColor = playerSprite.color;

        playerStun = GetComponent<PlayerStun>();
        playerSFX = GetComponent<PlayerSFX>();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;

            if (flashWhenDamaged && playerSprite != null)
            {
                flashTimer -= Time.deltaTime;

                if (flashTimer <= 0f)
                {
                    spriteHidden = !spriteHidden;

                    Color color = originalColor;
                    color.a = spriteHidden ? 0.35f : originalColor.a;

                    playerSprite.color = color;
                    flashTimer = flashInterval;
                }
            }

            if (invulnerabilityTimer <= 0f)
                RestoreSprite();
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || invulnerabilityTimer > 0f)
            return;

        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
            return;

        if (runData == null)
            runData = RunManager.Instance.Data;

        if (runData == null || runData.CurrentHearts <= 0)
            return;

        // Activate protection before notifying other systems of health changes.
        invulnerabilityTimer = invulnerabilityTime;
        flashTimer = 0f;

        runData.ChangeHearts(-damage);

        if (playerStun != null)
            playerStun.ApplyStun();

        Debug.Log(
            $"Player took {damage} damage! " +
            $"HP: {runData.CurrentHearts}/{runData.MaxHearts}"
        );

        if (runData.CurrentHearts <= 0)
            Die();

        runData.ChangeHearts(-damage);
        if (playerSFX != null)
            playerSFX.PlayHurt();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (runData == null)
            return;

        runData.ChangeHearts(amount);
    }

    public int CurrentHearts =>
        runData != null ? runData.CurrentHearts : 0;

    public int MaxHearts =>
        runData != null ? runData.MaxHearts : 0;

    public bool IsInvulnerable =>
        invulnerabilityTimer > 0f;

    private void Die()
    {
        Debug.Log("PLAYER HAS 0 HP!");

        // RunEndDetector handles ending the run,
        // converting trash and displaying Game Over.
    }

    private void RestoreSprite()
    {
        spriteHidden = false;

        if (playerSprite != null)
            playerSprite.color = originalColor;
    }

    private void OnDisable()
    {
        RestoreSprite();
        invulnerabilityTimer = 0f;
    }
}
