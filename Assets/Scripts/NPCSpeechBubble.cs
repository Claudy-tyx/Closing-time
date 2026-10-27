
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class NPCSpeechBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubble;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform bubbleRect;

    [Header("Animation")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("World SFX")]
    [SerializeField] private AudioClip bubblePopClip;
    [SerializeField] private AudioMixerGroup worldSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float bubblePopVolume = 0.6f;

    private AudioSource worldSFXSource;

    private Coroutine routine;
    private Vector3 originalScale;
    private Camera mainCamera;

    private void Awake()
    {
        worldSFXSource = gameObject.AddComponent<AudioSource>();
        worldSFXSource.playOnAwake = false;
        worldSFXSource.loop = false;
        worldSFXSource.spatialBlend = 0f;
        worldSFXSource.outputAudioMixerGroup = worldSFXMixerGroup;
        mainCamera = Camera.main;

        if (bubbleRect == null && bubble != null)
            bubbleRect = bubble.GetComponent<RectTransform>();

        if (canvasGroup == null && bubble != null)
            canvasGroup = bubble.GetComponent<CanvasGroup>();

        if (bubbleRect != null)
            originalScale = bubbleRect.localScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (bubble != null)
            bubble.SetActive(false);
    }

    public void Show(string message)
    {
        if (bubble == null || messageText == null ||
            canvasGroup == null || bubbleRect == null)
            return;

        bool wasVisible = bubble.activeSelf;

        if (routine != null)
            StopCoroutine(routine);

        messageText.text = message;
        bubble.SetActive(true);

        // Only play the pop when the bubble first appears.
        if (!wasVisible &&
            bubblePopClip != null &&
            worldSFXSource != null &&
            IsVisibleToCamera())
        {
            worldSFXSource.PlayOneShot(
                bubblePopClip,
                bubblePopVolume
            );
        }

        routine = StartCoroutine(AnimateBubble());
    }

    private bool IsVisibleToCamera()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return false;

        Vector3 viewportPoint =
            mainCamera.WorldToViewportPoint(transform.position);

        return viewportPoint.z > 0f &&
            viewportPoint.x >= 0f &&
            viewportPoint.x <= 1f &&
            viewportPoint.y >= 0f &&
            viewportPoint.y <= 1f;
    }

    private IEnumerator AnimateBubble()
    {
        canvasGroup.alpha = 1f;
        bubbleRect.localScale = originalScale * 0.7f;

        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            bubbleRect.localScale = Vector3.Lerp(
                originalScale * 0.7f,
                originalScale,
                Mathf.Clamp01(elapsed / popDuration)
            );

            yield return null;
        }

        bubbleRect.localScale = originalScale;

        yield return new WaitForSecondsRealtime(displayDuration);

        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            canvasGroup.alpha = 1f -
                Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        bubble.SetActive(false);
        routine = null;
    }
}
