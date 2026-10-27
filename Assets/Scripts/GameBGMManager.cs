using System.Collections;
using UnityEngine;

public class GameBGMManager : MonoBehaviour
{
    public static GameBGMManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;

    [Header("Music")]
    [SerializeField] private AudioClip defaultBGM;
    [SerializeField] private AudioClip gameOverBGM;

    [Header("Transitions")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private float normalVolume;
    private Coroutine fadeRoutine;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        if (bgmSource == null)
        {
            Debug.LogError("GameBGMManager: AudioSource missing!");
            enabled = false;
            return;
        }

        normalVolume = bgmSource.volume;
    }

    private void Start()
    {
        if (defaultBGM != null)
        {
            bgmSource.clip = defaultBGM;
            bgmSource.loop = true;
            bgmSource.volume = normalVolume;
            bgmSource.Play();
        }
    }

    public void PlayGameOver()
    {
        if (isGameOver || gameOverBGM == null)
            return;

        isGameOver = true;
        ChangeMusic(gameOverBGM, false);
    }

    public void PlayDistrictMusic(AudioClip districtBGM)
    {
        if (isGameOver || districtBGM == null)
            return;

        if (bgmSource.clip == districtBGM)
            return;

        ChangeMusic(districtBGM, true);
    }

    private void ChangeMusic(AudioClip newClip, bool loop)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(
            FadeToMusic(newClip, loop)
        );
    }

    private IEnumerator FadeToMusic(AudioClip newClip, bool loop)
    {
        float startingVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fadeOutDuration)
            );

            bgmSource.volume =
                Mathf.Lerp(startingVolume, 0f, t);

            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = loop;
        bgmSource.volume = 0f;
        bgmSource.Play();

        elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fadeInDuration)
            );

            bgmSource.volume =
                Mathf.Lerp(0f, normalVolume, t);

            yield return null;
        }

        bgmSource.volume = normalVolume;
        fadeRoutine = null;
    }
}