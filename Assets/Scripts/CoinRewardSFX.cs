using UnityEngine;

public class CoinRewardSFX : MonoBehaviour
{
    public static CoinRewardSFX Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip coinRewardClip;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayReward()
    {
        if (audioSource == null || coinRewardClip == null)
            return;

        audioSource.PlayOneShot(coinRewardClip, volume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}