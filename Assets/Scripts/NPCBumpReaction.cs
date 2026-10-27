
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCBumpReaction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCSpeechBubble speechBubble;
    [SerializeField] private Collider2D npcBodyCollider;

    [Header("Roaming Boundary (Optional)")]
    [SerializeField] private BoxCollider2D roamZone;
    [SerializeField] private float boundaryPadding = 0.05f;

    [Header("Player Damage")]
    [SerializeField] private int heartDamage = 1;
    [SerializeField] private float damageCooldown = 1.5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 0.3f;
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float returnDuration = 0.25f;

    private Rigidbody2D rb;
    private Coroutine bumpRoutine;
    private float nextDamageTime;

    public bool IsBumping => bumpRoutine != null;

    public bool IsInvulnerable =>
        IsBumping || Time.time < nextDamageTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (speechBubble == null)
            speechBubble =
                GetComponentInChildren<NPCSpeechBubble>(true);

        if (npcBodyCollider == null)
            npcBodyCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryBump(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBump(collision.collider);
    }

    private void TryBump(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (npcBodyCollider == null ||
            !npcBodyCollider.IsTouching(other))
            return;

        // Invulnerable during knockback and cooldown.
        if (IsInvulnerable)
            return;

        nextDamageTime = Time.time + damageCooldown;

        if (speechBubble != null)
            speechBubble.Show("Ouch!");

        if (RunManager.Instance != null &&
            RunManager.Instance.Data != null)
        {
            RunManager.Instance.Data.ChangeHearts(-heartDamage);
        }

        bumpRoutine = StartCoroutine(
            KnockbackAndReturn(other.transform.position)
        );
    }

    private IEnumerator KnockbackAndReturn(Vector3 playerPosition)
    {
        Vector2 start = rb.position;

        Vector2 direction = start - (Vector2)playerPosition;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.up;

        direction.Normalize();

        Vector2 pushedPosition =
            start + direction * knockbackDistance;

        // Keep the NPC inside its roaming boundary.
        // Only apply boundary clamping if the NPC
        // already starts inside its roaming zone.
        if (roamZone != null &&
            roamZone.bounds.Contains(
                new Vector3(start.x, start.y, roamZone.bounds.center.z)
            ))
        {
            pushedPosition = ClampToRoamZone(pushedPosition);
        }

        // Never allow the knockback to exceed its intended distance.
        Vector2 offset = pushedPosition - start;

        pushedPosition = start + Vector2.ClampMagnitude(
            offset,
            knockbackDistance
        );

        yield return MoveNPC(
            start,
            pushedPosition,
            knockbackDuration
        );

        yield return MoveNPC(
            pushedPosition,
            start,
            returnDuration
        );

        rb.MovePosition(start);

        yield return new WaitForFixedUpdate();

        // Cooldown begins after returning.
        nextDamageTime = Time.time + damageCooldown;

        bumpRoutine = null;
    }

    private Vector2 ClampToRoamZone(Vector2 position)
    {
        if (roamZone == null)
            return position;

        Bounds bounds = roamZone.bounds;

        Vector2 padding = Vector2.one * boundaryPadding;

        if (npcBodyCollider != null)
        {
            Vector3 extents = npcBodyCollider.bounds.extents;

            padding += new Vector2(extents.x, extents.y);
        }

        float minX = bounds.min.x + padding.x;
        float maxX = bounds.max.x - padding.x;
        float minY = bounds.min.y + padding.y;
        float maxY = bounds.max.y - padding.y;

        // Handle zones smaller than the NPC.
        if (minX > maxX)
            minX = maxX = bounds.center.x;

        if (minY > maxY)
            minY = maxY = bounds.center.y;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY)
        );
    }

    private IEnumerator MoveNPC(
        Vector2 from,
        Vector2 to,
        float duration)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            rb.MovePosition(Vector2.Lerp(from, to, t));

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(to);
        yield return new WaitForFixedUpdate();
    }
}
