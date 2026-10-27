
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RoadVehicleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class VehicleSpawnData
    {
        public GameObject prefab;

        [Min(0f)]
        public float spawnWeight = 10f;
    }

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Camera Activation")]
    [SerializeField] private float activationPadding = 5f;
    [SerializeField] private float despawnPadding = 10f;
    [SerializeField] private float cameraCheckInterval = 0.25f;

    [Header("Vehicles")]
    [SerializeField] private VehicleSpawnData[] vehicles;
    [SerializeField] private VehicleController.MoveDirection direction;

    [Header("Spawning")]
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private float maxSpawnDelay = 5f;
    [SerializeField] private int maxActiveVehicles = 8;

    [Header("References")]
    [SerializeField] private Transform vehicleParent;

    [Tooltip("How far outside the camera cars should spawn.")]
    [SerializeField] private float spawnPadding = 3f;

    private BoxCollider2D roadZone;
    private Coroutine spawnRoutine;
    private Coroutine cameraRoutine;
    private bool cameraInArea;

    private readonly List<GameObject> spawnedVehicles =
        new List<GameObject>();

    private void Awake()
    {
        roadZone = GetComponent<BoxCollider2D>();
        roadZone.isTrigger = true;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        cameraRoutine = StartCoroutine(CameraCheckRoutine());
    }

    private void OnDisable()
    {
        if (cameraRoutine != null)
        {
            StopCoroutine(cameraRoutine);
            cameraRoutine = null;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        cameraInArea = false;
    }

    private IEnumerator CameraCheckRoutine()
    {
        while (true)
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera != null)
            {
                bool shouldSpawn =
                    CameraOverlapsZone(activationPadding);

                if (shouldSpawn != cameraInArea)
                {
                    cameraInArea = shouldSpawn;

                    if (cameraInArea)
                    {
                        if (spawnRoutine == null)
                            spawnRoutine =
                                StartCoroutine(SpawnRoutine());
                    }
                    else
                    {
                        if (spawnRoutine != null)
                        {
                            StopCoroutine(spawnRoutine);
                            spawnRoutine = null;
                        }
                    }
                }

                // Remove distant cars so they cannot accumulate.
                if (!CameraOverlapsZone(despawnPadding))
                {
                    ClearSpawnedVehicles();
                }

                UpdateVehicles();
            }

            yield return new WaitForSeconds(
                Mathf.Max(0.05f, cameraCheckInterval)
            );
        }
    }

    private bool CameraOverlapsZone(float padding)
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return false;

        float halfHeight =
            targetCamera.orthographicSize + padding;

        float halfWidth =
            targetCamera.orthographicSize *
            targetCamera.aspect + padding;

        Vector2 cameraCenter =
            targetCamera.transform.position;

        Bounds bounds = roadZone.bounds;

        return
            cameraCenter.x + halfWidth >= bounds.min.x &&
            cameraCenter.x - halfWidth <= bounds.max.x &&
            cameraCenter.y + halfHeight >= bounds.min.y &&
            cameraCenter.y - halfHeight <= bounds.max.y;
    }

    private IEnumerator SpawnRoutine()
    {
        while (cameraInArea)
        {
            float delay = Random.Range(
                Mathf.Min(minSpawnDelay, maxSpawnDelay),
                Mathf.Max(minSpawnDelay, maxSpawnDelay)
            );

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, delay)
            );

            spawnedVehicles.RemoveAll(vehicle => vehicle == null);

            if (cameraInArea &&
                spawnedVehicles.Count < maxActiveVehicles)
            {
                SpawnVehicle();
            }
        }

        spawnRoutine = null;
    }

    
    private void SpawnVehicle()
    {
        GameObject prefab = PickVehicle();

        if (prefab == null || targetCamera == null)
            return;

        Bounds bounds = roadZone.bounds;

        Vector2 cameraCenter = targetCamera.transform.position;

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;

        float padding = Mathf.Max(0f, spawnPadding);

        Vector2 spawnPosition = bounds.center;

        switch (direction)
        {
            case VehicleController.MoveDirection.Right:
                spawnPosition.x = Mathf.Clamp(
                    cameraCenter.x - halfWidth - padding,
                    bounds.min.x,
                    bounds.max.x
                );

                spawnPosition.y = Random.Range(
                    bounds.min.y,
                    bounds.max.y
                );
                break;

            case VehicleController.MoveDirection.Left:
                spawnPosition.x = Mathf.Clamp(
                    cameraCenter.x + halfWidth + padding,
                    bounds.min.x,
                    bounds.max.x
                );

                spawnPosition.y = Random.Range(
                    bounds.min.y,
                    bounds.max.y
                );
                break;

            case VehicleController.MoveDirection.Up:
                spawnPosition.x = Random.Range(
                    bounds.min.x,
                    bounds.max.x
                );

                spawnPosition.y = Mathf.Clamp(
                    cameraCenter.y - halfHeight - padding,
                    bounds.min.y,
                    bounds.max.y
                );
                break;

            case VehicleController.MoveDirection.Down:
                spawnPosition.x = Random.Range(
                    bounds.min.x,
                    bounds.max.x
                );

                spawnPosition.y = Mathf.Clamp(
                    cameraCenter.y + halfHeight + padding,
                    bounds.min.y,
                    bounds.max.y
                );
                break;
        }

        GameObject spawned = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity,
            vehicleParent
        );

        VehicleController controller =
            spawned.GetComponent<VehicleController>();

        if (controller != null)
            controller.SetDirection(direction);

        spawnedVehicles.Add(spawned);
    }


    private void UpdateVehicles()
    {
        Bounds roadBounds = roadZone.bounds;

        for (int i = spawnedVehicles.Count - 1; i >= 0; i--)
        {
            GameObject vehicle = spawnedVehicles[i];

            if (vehicle == null)
            {
                spawnedVehicles.RemoveAt(i);
                continue;
            }

            BoxCollider2D bodyCollider =
                vehicle.GetComponent<BoxCollider2D>();

            if (bodyCollider == null)
                continue;

            Bounds vehicleBounds = bodyCollider.bounds;
            bool reachedEnd = false;

            switch (direction)
            {
                case VehicleController.MoveDirection.Right:
                    reachedEnd =
                        vehicleBounds.min.x > roadBounds.max.x;
                    break;

                case VehicleController.MoveDirection.Left:
                    reachedEnd =
                        vehicleBounds.max.x < roadBounds.min.x;
                    break;

                case VehicleController.MoveDirection.Up:
                    reachedEnd =
                        vehicleBounds.min.y > roadBounds.max.y;
                    break;

                case VehicleController.MoveDirection.Down:
                    reachedEnd =
                        vehicleBounds.max.y < roadBounds.min.y;
                    break;
            }

            if (reachedEnd)
            {
                Destroy(vehicle);
                spawnedVehicles.RemoveAt(i);
            }
        }
    }

    private void ClearSpawnedVehicles()
    {
        foreach (GameObject vehicle in spawnedVehicles)
        {
            if (vehicle != null)
                Destroy(vehicle);
        }

        spawnedVehicles.Clear();
    }

    private GameObject PickVehicle()
    {
        if (vehicles == null || vehicles.Length == 0)
            return null;

        float totalWeight = 0f;

        foreach (VehicleSpawnData data in vehicles)
        {
            if (data.prefab != null)
                totalWeight += Mathf.Max(0f, data.spawnWeight);
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);

        foreach (VehicleSpawnData data in vehicles)
        {
            if (data.prefab == null || data.spawnWeight <= 0f)
                continue;

            randomValue -= data.spawnWeight;

            if (randomValue <= 0f)
                return data.prefab;
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            box.bounds.center,
            box.bounds.size
        );
    }
}
