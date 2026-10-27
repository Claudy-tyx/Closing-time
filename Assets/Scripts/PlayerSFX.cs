
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSFX : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip wallBumpClip;
    [SerializeField] private AudioClip slipClip;
    [SerializeField] private AudioClip sneezeClip;

    [Header("Footsteps")]
    [SerializeField] private float walkStepInterval = 0.4f;
    [SerializeField] private float sprintStepInterval = 0.25f;
    [SerializeField] private float minimumMovementSpeed = 0.1f;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("Other Volumes")]
    [SerializeField] private float hurtVolume = 1f;
    [SerializeField] private float wallBumpVolume = 0.6f;
    [SerializeField] private float slipVolume = 0.8f;
    [SerializeField] private float sneezeVolume = 0.8f;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerSlip playerSlip;
    private PlayerStun playerStun;

    private float stepTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerSlip = GetComponent<PlayerSlip>();
        playerStun = GetComponent<PlayerStun>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        bool runActive =
            RunManager.Instance != null &&
            RunManager.Instance.IsRunActive;

        bool blocked =
            (playerSlip != null && playerSlip.IsSlipping) ||
            (playerStun != null && playerStun.IsStunned);

        bool moving =
            rb.linearVelocity.sqrMagnitude >
            minimumMovementSpeed * minimumMovementSpeed;

        if (!runActive || blocked || !moving)
        {
            stepTimer = 0f;
            return;
        }

        float interval =
            playerController != null && playerController.IsSprinting
                ? sprintStepInterval
                : walkStepInterval;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayRandomFootstep();
            stepTimer = Mathf.Max(0.05f, interval);
        }
    }

    private void PlayRandomFootstep()
    {
        AudioClip clip =
            footstepClips[Random.Range(0, footstepClips.Length)];

        PlayClip(clip, footstepVolume);
    }

    public void PlayHurt()
    {
        PlayClip(hurtClip, hurtVolume);
    }

    public void PlayWallBump()
    {
        PlayClip(wallBumpClip, wallBumpVolume);
    }

    public void PlaySlip()
    {
        PlayClip(slipClip, slipVolume);
    }

    public void PlaySneeze()
    {
        PlayClip(sneezeClip, sneezeVolume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}
