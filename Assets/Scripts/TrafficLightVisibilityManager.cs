
using System.Collections;
using UnityEngine;

public class TrafficLightVisibilityManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Activation Settings")]
    [SerializeField] private float activationPadding = 5f;

    [Tooltip("Seconds between visibility checks.")]
    [SerializeField] private float checkInterval = 0.15f;

    private TrafficLightController[] trafficLights;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        trafficLights =
            FindObjectsByType<TrafficLightController>(
                FindObjectsSortMode.None
            );

        StartCoroutine(CheckTrafficLights());
    }

    private IEnumerator CheckTrafficLights()
    {
        while (true)
        {
            if (targetCamera != null)
            {
                Vector3 cameraPosition =
                    targetCamera.transform.position;

                float halfHeight =
                    targetCamera.orthographicSize;

                float halfWidth =
                    halfHeight * targetCamera.aspect;

                float minX =
                    cameraPosition.x - halfWidth -
                    activationPadding;

                float maxX =
                    cameraPosition.x + halfWidth +
                    activationPadding;

                float minY =
                    cameraPosition.y - halfHeight -
                    activationPadding;

                float maxY =
                    cameraPosition.y + halfHeight +
                    activationPadding;

                foreach (TrafficLightController light in trafficLights)
                {
                    if (light == null)
                        continue;

                    Vector3 position =
                        light.transform.position;

                    bool isNearCamera =
                        position.x >= minX &&
                        position.x <= maxX &&
                        position.y >= minY &&
                        position.y <= maxY;

                    if (isNearCamera)
                        light.RefreshVisual();
                }
            }

            yield return new WaitForSeconds(
                Mathf.Max(0.02f, checkInterval)
            );
        }
    }
}
