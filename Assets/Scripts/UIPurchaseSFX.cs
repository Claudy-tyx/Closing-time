using UnityEngine;
using UnityEngine.Audio;

public class UIPurchaseSFX : MonoBehaviour
{
    public static UIPurchaseSFX Instance { get; private set; }

    [Header("Purchase Sound")]
    [SerializeField] private AudioClip purchaseClip;
    [SerializeField] private AudioMixerGroup uiSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = uiSFXMixerGroup;
    }

    public void PlayPurchase()
    {
        if (purchaseClip != null && audioSource != null)
            audioSource.PlayOneShot(purchaseClip, volume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}