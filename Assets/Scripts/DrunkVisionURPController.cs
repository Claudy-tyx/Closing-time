
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DrunkVisionURPController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material drunkVisionMaterial;
    [SerializeField] private Transform player;

    [Header("Sight Upgrade")]
    [SerializeField] private float[] clearRadiusByLevel =
    {
        0.15f,
        0.25f,
        0.40f,
        0.60f,
        1.50f
    };

    [SerializeField] private float[] blurStrengthByLevel =
    {
        4f,
        3f,
        2f,
        1f,
        0f
    };

    [Header("Transition")]
    [SerializeField] private float feather = 0.25f;
    [SerializeField] private float transitionSpeed = 3f;

    private Camera cam;
    private RunData runData;

    private float currentRadius;
    private float currentBlur;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        int level = GetSightLevel();

        currentRadius = clearRadiusByLevel[level];
        currentBlur = blurStrengthByLevel[level];
    }

    private void LateUpdate()
    {
        if (drunkVisionMaterial == null || player == null)
            return;

        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        bool runActive =
            RunManager.Instance != null &&
            RunManager.Instance.IsRunActive;

        int level = GetSightLevel();

        float targetRadius = runActive
            ? clearRadiusByLevel[level]
            : 1.5f;

        float targetBlur = runActive
            ? blurStrengthByLevel[level]
            : 0f;

        currentRadius = Mathf.Lerp(
            currentRadius,
            targetRadius,
            Time.deltaTime * transitionSpeed
        );

        currentBlur = Mathf.Lerp(
            currentBlur,
            targetBlur,
            Time.deltaTime * transitionSpeed
        );

        Vector3 viewportPosition =
            cam.WorldToViewportPoint(player.position);

        drunkVisionMaterial.SetVector(
            "_PlayerScreenPos",
            new Vector4(
                viewportPosition.x,
                viewportPosition.y,
                0f,
                0f
            )
        );

        drunkVisionMaterial.SetFloat(
            "_ClearRadius",
            currentRadius
        );

        drunkVisionMaterial.SetFloat(
            "_BlurStrength",
            currentBlur
        );

        drunkVisionMaterial.SetFloat(
            "_Feather",
            feather
        );
    }

    private int GetSightLevel()
    {
        if (runData == null)
            return 0;

        return Mathf.Clamp(
            runData.GetDistanceLevel(DistanceUpgrade.Sight),
            0,
            4
        );
    }
}
