using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScreenEffects : MonoBehaviour
{
    public static PlayerScreenEffects Instance { get; private set; }

    [Header("Overlay Images")]
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Image healOverlay;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float holdDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Maximum Opacity")]
    [Range(0f, 1f)]
    [SerializeField] private float hurtOpacity = 0.65f;

    [Range(0f, 1f)]
    [SerializeField] private float healOpacity = 0.45f;

    [Header("Optional SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSFX;
    [SerializeField] private AudioClip healSFX;

    private Coroutine effectRoutine;

    private void Awake()
    {
        Instance = this;

        SetAlpha(hurtOverlay, 0f);
        SetAlpha(healOverlay, 0f);

        if (hurtOverlay != null)
            hurtOverlay.raycastTarget = false;

        if (healOverlay != null)
            healOverlay.raycastTarget = false;
    }

    public void PlayHurt()
    {
        PlayEffect(hurtOverlay, hurtOpacity, hurtSFX);
    }

    public void PlayHeal()
    {
        PlayEffect(healOverlay, healOpacity, healSFX);
    }

    private void PlayEffect(
        Image overlay,
        float maxOpacity,
        AudioClip sound)
    {
        if (overlay == null)
            return;

        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        SetAlpha(hurtOverlay, 0f);
        SetAlpha(healOverlay, 0f);

        if (audioSource != null && sound != null)
            audioSource.PlayOneShot(sound);

        effectRoutine = StartCoroutine(
            AnimateEffect(overlay, maxOpacity)
        );
    }

    private IEnumerator AnimateEffect(
        Image overlay,
        float maxOpacity)
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fadeInDuration)
            );

            SetAlpha(overlay, Mathf.Lerp(0f, maxOpacity, t));

            yield return null;
        }

        SetAlpha(overlay, maxOpacity);

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        elapsed = 0f;

        // Fade out
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fadeOutDuration)
            );

            SetAlpha(overlay, Mathf.Lerp(maxOpacity, 0f, t));

            yield return null;
        }

        SetAlpha(overlay, 0f);
        effectRoutine = null;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}