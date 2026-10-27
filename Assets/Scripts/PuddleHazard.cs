
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PuddleHazard : MonoBehaviour
{
    [Header("Visual Variations")]
    [SerializeField] private Sprite[] puddleSprites;

    [Header("Slip Settings")]
    [SerializeField] private float slipDuration = 0.8f;
    [SerializeField] private float slipSpeed = 7f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null &&
            puddleSprites != null &&
            puddleSprites.Length > 0)
        {
            spriteRenderer.sprite = puddleSprites[
                Random.Range(0, puddleSprites.Length)
            ];
        }
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(
            $"[PUDDLE] Trigger entered by: {other.name}"
        );

        PlayerSlip player =
            other.GetComponentInParent<PlayerSlip>();

        if (player == null)
        {
            Debug.LogWarning(
                "[PUDDLE] PlayerSlip component not found!"
            );
            return;
        }

        Debug.Log("[PUDDLE] PlayerSlip found!");

        player.TrySlip(slipDuration, slipSpeed);
    }

}
