
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DrunkVisionEffect : MonoBehaviour
{
    [Header("Shader")]
    [SerializeField] private Shader blurShader;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Sight Upgrade Settings")]
    [SerializeField] private float[] clearRadiusByLevel =
    {
        0.15f,
        0.25f,
        0.4f,
        0.6f,
        1.5f
    };

    [SerializeField] private float[] blurStrengthByLevel =
    {
        4f,
        3f,
        2f,
        1f,
        0f
    };

    [Header("Blur Transition")]
    [SerializeField] private float feather = 0.25f;
    [SerializeField] private float transitionSpeed = 3f;

    private Material blurMaterial;
    private Camera cam;
    private RunData runData;

    private float currentRadius;
    private float currentBlur;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (blurShader != null && blurShader.isSupported)
        {
            blurMaterial = new Material(blurShader);
            blurMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        int level = GetSightLevel();

        currentRadius = clearRadiusByLevel[level];
        currentBlur = blurStrengthByLevel[level];
    }

    private void Update()
    {
        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        int level = GetSightLevel();

        currentRadius = Mathf.Lerp(
            currentRadius,
            clearRadiusByLevel[level],
            Time.deltaTime * transitionSpeed
        );

        currentBlur = Mathf.Lerp(
            currentBlur,
            blurStrengthByLevel[level],
            Time.deltaTime * transitionSpeed
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

    private void OnRenderImage(
        RenderTexture source,
        RenderTexture destination)
    {
        if (blurMaterial == null || player == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Disable effect outside active runs.
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Vector3 viewportPosition =
            cam.WorldToViewportPoint(player.position);

        blurMaterial.SetVector(
            "_PlayerScreenPos",
            new Vector4(
                viewportPosition.x,
                viewportPosition.y,
                0f,
                0f
            )
        );
        

        blurMaterial.SetFloat("_ClearRadius", currentRadius);
        blurMaterial.SetFloat("_BlurStrength", currentBlur);
        blurMaterial.SetFloat("_Feather", feather);

        Graphics.Blit(source, destination, blurMaterial);
    }

    private void OnDestroy()
    {
        if (blurMaterial != null)
            Destroy(blurMaterial);
    }

    
}
