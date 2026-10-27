
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Spawn Settings")]
    [SerializeField, Min(0)] private int maxCoins = 10;
    [SerializeField, Min(0f)] private float minDistance = 1f;
    [SerializeField, Min(1)] private int maxAttemptsPerCoin = 30;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0f)] private float obstacleCheckRadius = 0.25f;

    private BoxCollider2D spawnArea;
    private readonly List<GameObject> spawnedCoins =
        new List<GameObject>();

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        spawnArea.isTrigger = true;
    }

    private void Start()
    {
        SpawnCoins();
    }

    public void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning(
                "CoinSpawner: No coin prefab assigned.",
                this
            );
            return;
        }

        ClearCoins();

        List<Vector2> positions = new List<Vector2>();

        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < maxCoins; i++)
        {
            bool foundPosition = false;

            for (int attempt = 0;
                 attempt < maxAttemptsPerCoin;
                 attempt++)
            {
                Vector2 position = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

                // Ensure point lies inside the actual collider.
                if (!spawnArea.OverlapPoint(position))
                    continue;

                // Avoid other coins.
                bool tooClose = false;

                foreach (Vector2 existing in positions)
                {
                    if (Vector2.Distance(position, existing)
                        < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                // Avoid walls/buildings.
                if (Physics2D.OverlapCircle(
                    position,
                    obstacleCheckRadius,
                    obstacleLayers
                ) != null)
                {
                    continue;
                }

                positions.Add(position);
                foundPosition = true;
                break;
            }

            if (!foundPosition)
                continue;

            GameObject coin = Instantiate(
                coinPrefab,
                positions[positions.Count - 1],
                Quaternion.identity,
                transform
            );

            spawnedCoins.Add(coin);
        }
    }

    public void ClearCoins()
    {
        foreach (GameObject coin in spawnedCoins)
        {
            if (coin != null)
                Destroy(coin);
        }

        spawnedCoins.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box == null)
            return;

        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(box.offset, box.size);

        Gizmos.matrix = oldMatrix;
    }
}
