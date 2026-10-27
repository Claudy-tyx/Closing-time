
using UnityEngine;

public class VehicleBodyDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int heartDamage = 1;

    [Header("References")]
    [SerializeField] private BoxCollider2D bodyCollider;

    private void Awake()
    {
        if (bodyCollider == null)
            bodyCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (bodyCollider == null || other == null)
            return;

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
            return;

        // Damage only when the BODY collider overlaps the player.
        if (!bodyCollider.IsTouching(other))
            return;

        health.TakeDamage(heartDamage);
    }
}
