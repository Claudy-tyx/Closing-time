
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Value")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private RunData runData;

    [Header("Visuals")]
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private Transform collectVisual;
    [SerializeField] private Animator collectAnimator;

    [Header("Collection Animation")]
    [SerializeField] private float collectDuration = 0.35f;
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float collectSpinSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField, Range(0f, 1f)]
    private float soundVolume = 0.8f;

    [Header("Pitch Combo")]
    [SerializeField] private float pitchIncrease = 0.1f;
    [SerializeField] private int maxPitchLevel = 5;
    [SerializeField] private float comboResetTime = 1.5f;

    private static int comboCount = 0;
    private static float lastCollectTime = -999f;

    private bool collected;
    private Collider2D pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();

        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (collectVisual != null)
            collectVisual.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        Collect();
    }

    private void Collect()
    {
        if (runData == null)
        {
            Debug.LogWarning(
                "CoinPickup: RunData is missing.",
                this
            );
            return;
        }

        collected = true;

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        runData.AddCoins(coinValue);

        // Hide ground coin immediately.
        if (idleVisual != null)
            idleVisual.SetActive(false);

        // Activate separate pickup animation.
        if (collectVisual != null)
            collectVisual.gameObject.SetActive(true);

        PlayCollectSound();

        StartCoroutine(PlayCollectAnimation());
    }

    private void PlayCollectSound()
    {
        if (Time.time - lastCollectTime > comboResetTime)
            comboCount = 0;

        comboCount = Mathf.Min(
            comboCount + 1,
            Mathf.Max(1, maxPitchLevel)
        );

        lastCollectTime = Time.time;

        float pitch = 1f + (comboCount - 1) * pitchIncrease;

        if (collectSound == null)
            return;

        GameObject audioObject = new GameObject("CoinPickupSFX");
        audioObject.transform.position = transform.position;

        AudioSource source = audioObject.AddComponent<AudioSource>();

        source.clip = collectSound;
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.volume = soundVolume;
        source.pitch = pitch;
        source.spatialBlend = 0f; // 2D sound

        source.Play();

        // Keep the sound alive after the coin is destroyed.
        float lifetime = collectSound.length /
            Mathf.Max(0.01f, Mathf.Abs(pitch));

        Destroy(audioObject, lifetime + 0.1f);
    }

    private IEnumerator PlayCollectAnimation()
    {
        if (collectVisual == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 startPosition = collectVisual.localPosition;

        if (collectAnimator == null)
            collectAnimator =
                collectVisual.GetComponent<Animator>();

        if (collectAnimator != null)
            collectAnimator.speed = collectSpinSpeed;

        float duration = Mathf.Max(0.01f, collectDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Hop upward and settle slightly.
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            collectVisual.localPosition =
                startPosition + Vector3.up * height;

            yield return null;
        }

        Destroy(gameObject);
    }
}
