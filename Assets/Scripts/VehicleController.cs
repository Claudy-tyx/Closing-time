
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public enum MoveDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private VehicleFrontSensor frontSensor;
    [SerializeField] private Transform frontSensorTransform;
    [SerializeField] private BoxCollider2D bodyCollider;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;

    [Header("Automatic Collider Sizing")]
    [Tooltip("Extra padding on each side of the sprite.")]
    [SerializeField] private Vector2 colliderPadding =
        new Vector2(0.05f, 0.05f);

    [Header("Front Sensor Settings")]
    [SerializeField] private float sensorGap = 0.05f;
    [SerializeField] private float sensorThickness = 0.2f;
    [SerializeField] private float sensorWidthMultiplier = 0.85f;

    private MoveDirection moveDirection = MoveDirection.Right;
    private Vector2 movementVector = Vector2.right;

    private bool stoppedAtCrossing;

    public MoveDirection CurrentDirection => moveDirection;
    public Vector2 ForwardDirection => movementVector;

    public bool IsWaitingInQueue { get; private set; }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (frontSensor == null)
            frontSensor = GetComponentInChildren<VehicleFrontSensor>();

        if (frontSensorTransform == null && frontSensor != null)
            frontSensorTransform = frontSensor.transform;

        if (bodyCollider == null)
            bodyCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        bool queuedCarAhead =
            frontSensor != null &&
            frontSensor.HasQueuedVehicleAhead;

        IsWaitingInQueue = stoppedAtCrossing || queuedCarAhead;

        if (IsWaitingInQueue)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movementVector * moveSpeed;
    }

    private void UpdateDirectionalColliders()
    {
        if (spriteRenderer == null ||
            spriteRenderer.sprite == null ||
            bodyCollider == null)
            return;

        Sprite currentSprite = spriteRenderer.sprite;

        // Get sprite dimensions in Unity units.
        Vector2 spriteSize = currentSprite.bounds.size;

        // Automatically resize the vehicle's body collider.
        bodyCollider.size = new Vector2(
            Mathf.Max(
                0.01f,
                spriteSize.x + colliderPadding.x * 2f
            ),
            Mathf.Max(
                0.01f,
                spriteSize.y + colliderPadding.y * 2f
            )
        );

        // Align the collider with the sprite.
        bodyCollider.offset = currentSprite.bounds.center;

        if (frontSensorTransform == null)
            return;

        BoxCollider2D sensorCollider =
            frontSensorTransform.GetComponent<BoxCollider2D>();

        if (sensorCollider == null)
            return;

        bool horizontal =
            moveDirection == MoveDirection.Left ||
            moveDirection == MoveDirection.Right;

        // Automatically resize the front sensor.
        if (horizontal)
        {
            sensorCollider.size = new Vector2(
                sensorThickness,
                bodyCollider.size.y * sensorWidthMultiplier
            );
        }
        else
        {
            sensorCollider.size = new Vector2(
                bodyCollider.size.x * sensorWidthMultiplier,
                sensorThickness
            );
        }
    }

    public void SetDirection(MoveDirection direction)
    {
        moveDirection = direction;

        switch (direction)
        {
            case MoveDirection.Right:
                movementVector = Vector2.right;
                spriteRenderer.sprite = spriteRight;
                break;

            case MoveDirection.Left:
                movementVector = Vector2.left;
                spriteRenderer.sprite = spriteLeft;
                break;

            case MoveDirection.Up:
                movementVector = Vector2.up;
                spriteRenderer.sprite = spriteUp;
                break;

            case MoveDirection.Down:
                movementVector = Vector2.down;
                spriteRenderer.sprite = spriteDown;
                break;
        }

        transform.rotation = Quaternion.identity;

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;

        UpdateDirectionalColliders();
        UpdateFrontSensor();
    }

    private void UpdateFrontSensor()
    {
        if (frontSensorTransform == null || bodyCollider == null)
            return;

        BoxCollider2D sensorCollider =
            frontSensorTransform.GetComponent<BoxCollider2D>();

        if (sensorCollider == null)
            return;

        Vector2 bodyHalf = bodyCollider.size * 0.5f;
        Vector2 sensorHalf = sensorCollider.size * 0.5f;

        Vector2 position = bodyCollider.offset;

        switch (moveDirection)
        {
            case MoveDirection.Right:
                position.x += bodyHalf.x + sensorHalf.x + sensorGap;
                break;

            case MoveDirection.Left:
                position.x -= bodyHalf.x + sensorHalf.x + sensorGap;
                break;

            case MoveDirection.Up:
                position.y += bodyHalf.y + sensorHalf.y + sensorGap;
                break;

            case MoveDirection.Down:
                position.y -= bodyHalf.y + sensorHalf.y + sensorGap;
                break;
        }

        frontSensorTransform.localPosition = position;
    }

    public void StopAtCrossing()
    {
        stoppedAtCrossing = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void ContinueDriving()
    {
        stoppedAtCrossing = false;
    }
}
