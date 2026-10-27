
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CatAllergyZone : MonoBehaviour
{
    [Header("Allergy Settings")]
    [SerializeField] private int heartDamage = 1;
    [SerializeField] private float allergyCooldown = 3f;

    [Header("Speech")]
    [SerializeField] private string sneezeMessage = "Ah choo!";

    private CircleCollider2D allergyCollider;

    // Track players currently inside the allergy zone.
    private readonly HashSet<PlayerHealth> playersInside =
        new HashSet<PlayerHealth>();

    private readonly Dictionary<PlayerHealth, float> nextAllergyTime =
        new Dictionary<PlayerHealth, float>();

    private void Awake()
    {
        allergyCollider = GetComponent<CircleCollider2D>();
        allergyCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        // Prevent multiple colliders from triggering the same cat.
        if (!playersInside.Add(player))
            return;

        TryTriggerAllergy(player);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        playersInside.Remove(player);
    }

    private void TryTriggerAllergy(PlayerHealth player)
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
            return;

        RunData runData = RunManager.Instance.Data;

        if (runData == null)
            return;

        // Purchased immunity prevents the allergy entirely.
        if (runData.AllergenImmunityUnlocked)
            return;

        if (nextAllergyTime.TryGetValue(
            player,
            out float nextAllowedTime))
        {
            if (Time.time < nextAllowedTime)
                return;
        }

        nextAllergyTime[player] =
            Time.time + allergyCooldown;

        PlayerSFX playerSFX = player.GetComponent<PlayerSFX>();

        if (playerSFX != null)
            playerSFX.PlaySneeze();

        // Apply damage through the existing health system.
        player.TakeDamage(heartDamage);

        // Display the player's existing speech bubble.
        PlayerSpeechBubble speechBubble =
            player.GetComponentInChildren<PlayerSpeechBubble>();

        if (speechBubble != null)
            speechBubble.ShowNotification(sneezeMessage);

        Debug.Log(
            $"Cat allergy triggered! Damage: {heartDamage}"
        );
    }

    private void OnDisable()
    {
        playersInside.Clear();
        nextAllergyTime.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D circle =
            GetComponent<CircleCollider2D>();

        if (circle == null)
            return;

        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.8f);

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireSphere(
            circle.offset,
            circle.radius
        );
    }
}
