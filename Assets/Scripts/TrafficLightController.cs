
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Green,
        Transitioning
    }

    [Header("Cycle Timing")]
    [SerializeField] private float redDuration = 8f;
    [SerializeField] private float greenDuration = 8f;
    [SerializeField] private float transitionDuration = 1f;

    [Header("Animated Light")]
    [SerializeField] private SpriteRenderer lightSpriteRenderer;
    [SerializeField] private Sprite[] transitionFrames;

    [Header("Starting Offset")]
    [SerializeField] private bool randomStartOffset = true;
    [SerializeField] private float manualStartOffset = 0f;

    private float startOffset;

    private float CycleLength =>
        Mathf.Max(0.1f, redDuration) +
        Mathf.Max(0.1f, greenDuration) +
        Mathf.Max(0.01f, transitionDuration) * 2f;

    private float CycleTime =>
        Mathf.Repeat(Time.time + startOffset, CycleLength);

    public LightState CurrentState
    {
        get
        {
            float t = CycleTime;
            float redEnd = redDuration;
            float forwardEnd = redEnd + transitionDuration;
            float greenEnd = forwardEnd + greenDuration;

            if (t < redEnd)
                return LightState.Red;

            if (t < forwardEnd)
                return LightState.Transitioning;

            if (t < greenEnd)
                return LightState.Green;

            return LightState.Transitioning;
        }
    }

    public bool CarsMustStop =>
        CurrentState != LightState.Green;

    private void Awake()
    {
        redDuration = Mathf.Max(0.1f, redDuration);
        greenDuration = Mathf.Max(0.1f, greenDuration);
        transitionDuration = Mathf.Max(0.01f, transitionDuration);

        startOffset = randomStartOffset
            ? Random.Range(0f, CycleLength)
            : Mathf.Repeat(manualStartOffset, CycleLength);
    }

    private void Start()
    {
        RefreshVisual();
    }

    // Called only when the light is near the camera.
    public void RefreshVisual()
    {
        if (lightSpriteRenderer == null ||
            transitionFrames == null ||
            transitionFrames.Length == 0)
            return;

        float t = CycleTime;
        float redEnd = redDuration;
        float forwardEnd = redEnd + transitionDuration;
        float greenEnd = forwardEnd + greenDuration;

        float normalizedTime;

        if (t < redEnd)
        {
            normalizedTime = 0f;
        }
        else if (t < forwardEnd)
        {
            normalizedTime =
                (t - redEnd) / transitionDuration;
        }
        else if (t < greenEnd)
        {
            normalizedTime = 1f;
        }
        else
        {
            normalizedTime = 1f -
                (t - greenEnd) / transitionDuration;
        }

        int index = Mathf.Clamp(
            Mathf.FloorToInt(
                Mathf.Clamp01(normalizedTime) *
                transitionFrames.Length
            ),
            0,
            transitionFrames.Length - 1
        );

        lightSpriteRenderer.sprite = transitionFrames[index];
    }
}
