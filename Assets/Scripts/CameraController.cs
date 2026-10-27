
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Dead Zone (World Units)")]
    [SerializeField] private float deadZoneWidth = 2f;
    [SerializeField] private float deadZoneHeight = 1.5f;

    [Header("Camera Smoothing")]
    [SerializeField] private float smoothTime = 0.18f;

    [Header("Camera Offset")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("Pixel Art")]
    [SerializeField] private bool pixelSnap = false;
    [SerializeField] private int pixelsPerUnit = 32;

    private Vector3 velocity;
    private float cameraZ;

    private void Awake()
    {
        cameraZ = transform.position.z;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;
        }

        if (target != null)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 current = transform.position;

        Vector2 playerPosition =
            (Vector2)target.position + offset;

        float halfWidth = Mathf.Max(0f, deadZoneWidth) * 0.5f;
        float halfHeight = Mathf.Max(0f, deadZoneHeight) * 0.5f;

        float desiredX = current.x;
        float desiredY = current.y;

        // Only move when player leaves horizontal dead zone
        if (playerPosition.x > current.x + halfWidth)
            desiredX = playerPosition.x - halfWidth;
        else if (playerPosition.x < current.x - halfWidth)
            desiredX = playerPosition.x + halfWidth;

        // Only move when player leaves vertical dead zone
        if (playerPosition.y > current.y + halfHeight)
            desiredY = playerPosition.y - halfHeight;
        else if (playerPosition.y < current.y - halfHeight)
            desiredY = playerPosition.y + halfHeight;

        Vector3 desiredPosition = new Vector3(
            desiredX,
            desiredY,
            cameraZ
        );

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            current,
            desiredPosition,
            ref velocity,
            Mathf.Max(0.01f, smoothTime)
        );

        if (pixelSnap && pixelsPerUnit > 0)
        {
            float unit = 1f / pixelsPerUnit;

            smoothedPosition.x =
                Mathf.Round(smoothedPosition.x / unit) * unit;

            smoothedPosition.y =
                Mathf.Round(smoothedPosition.y / unit) * unit;
        }

        transform.position = smoothedPosition;
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        Vector2 position =
            (Vector2)target.position + offset;

        transform.position = new Vector3(
            position.x,
            position.y,
            cameraZ
        );

        velocity = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            new Vector3(
                transform.position.x,
                transform.position.y,
                0f
            ),
            new Vector3(
                deadZoneWidth,
                deadZoneHeight,
                0f
            )
        );
    }
}
