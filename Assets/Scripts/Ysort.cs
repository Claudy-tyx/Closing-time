using UnityEngine;

public class YSort : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform sortPoint;

    [Header("Settings")]
    [SerializeField] private bool isMovingObject = false;
    [SerializeField] private int sortingOffset = 0;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (sortPoint == null)
            sortPoint = transform;

        UpdateSortingOrder();

        // Static objects don't need LateUpdate.
        if (!isMovingObject)
            enabled = false;
    }

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    private void UpdateSortingOrder()
    {
        if (spriteRenderer == null || sortPoint == null)
            return;

        int newOrder =
            Mathf.RoundToInt(-sortPoint.position.y * 100f)
            + sortingOffset;

        // Avoid unnecessary SpriteRenderer updates.
        if (spriteRenderer.sortingOrder != newOrder)
            spriteRenderer.sortingOrder = newOrder;
    }
}