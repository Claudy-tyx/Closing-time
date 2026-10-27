
using System.Collections.Generic;
using UnityEngine;

public class PuddleSpawner : MonoBehaviour
{
    [Header("Puddle Prefab")]
    [SerializeField] private GameObject puddlePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int puddleCount = 20;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private Vector2 spawnAreaSize =
        new Vector2(20f, 20f);

    [Header("Spacing")]
    [SerializeField] private float minimumSpacing = 2f;
    [SerializeField] private int maxSpawnAttempts = 500;

    [Header("Object Layer")]
    [SerializeField] private bool overrideObjectLayer = false;
    [SerializeField] private LayerMask objectLayer;

    [Header("Sprite Sorting")]
    [SerializeField] private bool overrideSortingLayer = false;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int orderInLayer = 0;

    [Header("Hierarchy")]
    [SerializeField] private Transform puddleParent;

    private readonly List<GameObject> spawnedPuddles =
        new List<GameObject>();

    private void Start()
    {
        if (spawnOnStart)
            SpawnPuddles();
    }

    [ContextMenu("Spawn Puddles")]
    public void SpawnPuddles()
    {
        if (puddlePrefab == null)
        {
            Debug.LogWarning(
                "PuddleSpawner: No puddle prefab assigned!"
            );
            return;
        }

        ClearPuddles();

        List<Vector2> positions = new List<Vector2>();

        int attempts = 0;
        int spawned = 0;

        while (spawned < puddleCount &&
               attempts < maxSpawnAttempts)
        {
            attempts++;

            Vector2 randomOffset = new Vector2(
                Random.Range(
                    -spawnAreaSize.x / 2f,
                    spawnAreaSize.x / 2f
                ),
                Random.Range(
                    -spawnAreaSize.y / 2f,
                    spawnAreaSize.y / 2f
                )
            );

            Vector2 spawnPosition =
                (Vector2)transform.position + randomOffset;

            bool tooClose = false;

            foreach (Vector2 existingPosition in positions)
            {
                if (Vector2.Distance(
                    spawnPosition,
                    existingPosition
                ) < minimumSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            GameObject puddle = Instantiate(
                puddlePrefab,
                spawnPosition,
                Quaternion.identity,
                puddleParent != null
                    ? puddleParent
                    : transform
            );

            // Optionally override the Unity physics layer.
            if (overrideObjectLayer)
            {
                int layer = GetSelectedLayer();

                if (layer >= 0)
                    SetLayerRecursively(puddle, layer);
            }

            // Optionally override sprite sorting.
            if (overrideSortingLayer)
            {
                SpriteRenderer[] renderers =
                    puddle.GetComponentsInChildren<SpriteRenderer>();

                foreach (SpriteRenderer renderer in renderers)
                {
                    renderer.sortingLayerName = sortingLayerName;
                    renderer.sortingOrder = orderInLayer;
                }
            }

            spawnedPuddles.Add(puddle);
            positions.Add(spawnPosition);
            spawned++;
        }

        Debug.Log(
            $"PuddleSpawner: Spawned {spawned}/{puddleCount} puddles."
        );
    }

    private int GetSelectedLayer()
    {
        int mask = objectLayer.value;

        // LayerMask should contain exactly one layer.
        if (mask == 0 || (mask & (mask - 1)) != 0)
        {
            Debug.LogWarning(
                "PuddleSpawner: Select exactly one Object Layer."
            );
            return -1;
        }

        int layer = 0;

        while ((mask >>= 1) != 0)
            layer++;

        return layer;
    }

    private void SetLayerRecursively(
        GameObject obj,
        int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    [ContextMenu("Clear Puddles")]
    public void ClearPuddles()
    {
        foreach (GameObject puddle in spawnedPuddles)
        {
            if (puddle != null)
                Destroy(puddle);
        }

        spawnedPuddles.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(
                spawnAreaSize.x,
                spawnAreaSize.y,
                0f
            )
        );
    }
}
