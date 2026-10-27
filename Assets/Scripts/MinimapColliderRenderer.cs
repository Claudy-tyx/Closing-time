
using System.Collections.Generic;
using UnityEngine;

public class MinimapColliderRenderer : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask sourceLayers;
    [SerializeField] private string minimapLayerName = "MinimapOnly";

    [Header("Appearance")]
    [SerializeField] private Color colliderColor = Color.black;
    [SerializeField] private int sortingOrder = 1000;

    [Header("Filtering")]
    [SerializeField] private bool includeTriggers = false;

    private readonly Dictionary<BoxCollider2D, Transform> markers = new();

    private Sprite rectangleSprite;
    private Material markerMaterial;
    private int minimapLayer;

    private void Awake()
    {
        minimapLayer = LayerMask.NameToLayer(minimapLayerName);

        if (minimapLayer < 0)
        {
            Debug.LogError(
                $"Minimap layer '{minimapLayerName}' does not exist."
            );

            enabled = false;
            return;
        }

        Texture2D texture = Texture2D.whiteTexture;

        rectangleSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            1f
        );

        markerMaterial = new Material(
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
        );

        BuildMarkers();
    }

    private void BuildMarkers()
    {
        BoxCollider2D[] colliders =
            FindObjectsByType<BoxCollider2D>(
                FindObjectsSortMode.None
            );

        foreach (BoxCollider2D box in colliders)
        {
            if (!ShouldInclude(box))
                continue;

            CreateMarker(box);
        }
    }

    private bool ShouldInclude(BoxCollider2D box)
    {
        if (box == null || !box.enabled ||
            !box.gameObject.activeInHierarchy)
            return false;

        if (!includeTriggers && box.isTrigger)
            return false;

        return (sourceLayers.value &
                (1 << box.gameObject.layer)) != 0;
    }

    private void CreateMarker(BoxCollider2D box)
    {
        GameObject marker = new GameObject(
            "Minimap_" + box.name
        );

        marker.layer = minimapLayer;
        marker.transform.SetParent(transform);

        SpriteRenderer sr =
            marker.AddComponent<SpriteRenderer>();

        sr.sprite = rectangleSprite;
        sr.color = colliderColor;
        sr.material = markerMaterial;

        // Sorting Layer (must exist in Unity)
        sr.sortingLayerName = "MinimapLayer";
        sr.sortingOrder = 1000;

        markers.Add(box, marker.transform);

        UpdateMarker(box, marker.transform);
    }

    private void LateUpdate()
    {
        foreach (var pair in markers)
        {
            if (pair.Key == null)
            {
                if (pair.Value != null)
                    pair.Value.gameObject.SetActive(false);

                continue;
            }

            bool visible = ShouldInclude(pair.Key);

            if (pair.Value.gameObject.activeSelf != visible)
                pair.Value.gameObject.SetActive(visible);

            if (visible)
                UpdateMarker(pair.Key, pair.Value);
        }
    }

    private void UpdateMarker(
        BoxCollider2D box,
        Transform marker
    )
    {
        Vector3 center = box.transform.TransformPoint(
            new Vector3(box.offset.x, box.offset.y, 0f)
        );

        marker.position = new Vector3(
            center.x,
            center.y,
            0f
        );

        marker.rotation = box.transform.rotation;

        // Calculate the collider's dimensions in world units.
        Vector2 worldSize = Vector2.Scale(
            box.size,
            new Vector2(
                Mathf.Abs(box.transform.lossyScale.x),
                Mathf.Abs(box.transform.lossyScale.y)
            )
        );

        // Account for the generated rectangle sprite's actual size.
        Vector2 spriteSize = rectangleSprite.bounds.size;

        marker.localScale = new Vector3(
            worldSize.x / spriteSize.x,
            worldSize.y / spriteSize.y,
            1f
        );
    }

    private void OnDestroy()
    {
        if (markerMaterial != null)
            Destroy(markerMaterial);

        if (rectangleSprite != null)
            Destroy(rectangleSprite);
    }
}
