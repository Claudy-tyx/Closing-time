
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerSpeechTrigger : MonoBehaviour
{
    [Header("Speech Settings")]
    [TextArea(2, 5)]
    [SerializeField] private string message =
        "Where am I...?";

    [Min(0f)]
    [SerializeField] private float displayDuration = 3f;

    [Header("Trigger Settings")]
    [SerializeField] private bool oncePerRun = true;
    [SerializeField] private bool requireActiveRun = true;

    [Header("Speech Bubble")]
    [SerializeField] private PlayerSpeechBubble speechBubble;

    [Header("Pop SFX")]
    [SerializeField] private AudioClip popClip;
    [SerializeField] private AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    [SerializeField] private float popVolume = 0.6f;

    private AudioSource audioSource;
    private bool hasTriggered;

    private void Awake()
    {
        BoxCollider2D trigger =
            GetComponent<BoxCollider2D>();

        trigger.isTrigger = true;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = mixerGroup;
    }

    private void OnEnable()
    {
        RunManager.OnNewRunStarted += ResetForNewRun;
    }

    private void OnDisable()
    {
        RunManager.OnNewRunStarted -= ResetForNewRun;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oncePerRun && hasTriggered)
            return;

        if (requireActiveRun &&
            (RunManager.Instance == null ||
             !RunManager.Instance.IsRunActive))
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (speechBubble == null)
        {
            speechBubble =
                player.GetComponentInChildren<PlayerSpeechBubble>(
                    true
                );
        }

        if (speechBubble == null)
        {
            Debug.LogWarning(
                $"Speech trigger {gameObject.name}: " +
                "PlayerSpeechBubble not found."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
            return;

        hasTriggered = true;

        // Display the player's small speech bubble.
        // Disable its default pop sound to avoid duplicates.
        speechBubble.ShowNotification(
            message,
            null,
            displayDuration,
            false
        );

        // Play this trigger's custom pop SFX.
        if (popClip != null && audioSource != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.PlayOneShot(popClip, popVolume);
        }
    }

    private void ResetForNewRun()
    {
        hasTriggered = false;
    }
}
