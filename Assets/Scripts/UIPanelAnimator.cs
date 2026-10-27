using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform animatedPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float openDuration = 0.25f;
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private float slideDistance = 40f;
    [SerializeField] private float startScale = 0.85f;
    [SerializeField] private float overshootScale = 1.05f;

    [Header("UI SFX")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioMixerGroup uiSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float openVolume = 0.7f;

    private AudioSource audioSource;
    private Coroutine animationRoutine;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    private bool isOpen;

    private void Awake()
    {
        if (animatedPanel == null)
            animatedPanel = transform as RectTransform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPosition = animatedPanel.anchoredPosition;
        originalScale = animatedPanel.localScale;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = uiSFXMixerGroup;

        // Start hidden, but keep the GameObject active.
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        animatedPanel.localScale = originalScale * startScale;
        animatedPanel.anchoredPosition =
            originalPosition + Vector2.down * slideDistance;
    }

    public void Open()
    {
        if (isOpen)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        isOpen = true;

        if (openClip != null)
            audioSource.PlayOneShot(openClip, openVolume);

        animationRoutine = StartCoroutine(AnimateOpen());
    }

    public void Close()
    {
        if (!isOpen)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        isOpen = false;

        animationRoutine = StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateOpen()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector2 startPosition =
            originalPosition + Vector2.down * slideDistance;

        animatedPanel.anchoredPosition = startPosition;
        animatedPanel.localScale = originalScale * startScale;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, openDuration)
            );

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            animatedPanel.anchoredPosition =
                Vector2.Lerp(startPosition, originalPosition, eased);

            float scale = t < 0.7f
                ? Mathf.Lerp(startScale, overshootScale, t / 0.7f)
                : Mathf.Lerp(overshootScale, 1f, (t - 0.7f) / 0.3f);

            animatedPanel.localScale = originalScale * scale;
            canvasGroup.alpha = t;

            yield return null;
        }

        animatedPanel.anchoredPosition = originalPosition;
        animatedPanel.localScale = originalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        animationRoutine = null;
    }

    private IEnumerator AnimateClose()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector2 startPosition = animatedPanel.anchoredPosition;
        Vector3 startLocalScale = animatedPanel.localScale;
        float startAlpha = canvasGroup.alpha;

        Vector2 endPosition =
            originalPosition + Vector2.down * slideDistance;

        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, closeDuration)
            );

            float eased = t * t;

            animatedPanel.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, eased);

            animatedPanel.localScale =
                Vector3.Lerp(
                    startLocalScale,
                    originalScale * startScale,
                    eased
                );

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);

            yield return null;
        }

        animatedPanel.anchoredPosition =
            originalPosition + Vector2.down * slideDistance;

        animatedPanel.localScale = originalScale * startScale;
        canvasGroup.alpha = 0f;

        animationRoutine = null;
    }
}