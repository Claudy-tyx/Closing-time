
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

    [Header("Drunk Footstep Variations")]
    [SerializeField] private bool irregularCadence = true;
    [Range(0f, 0.4f)] [SerializeField] private float cadenceVariance = 0.3f;
    [Range(0f, 0.3f)] [SerializeField] private float stutterStepChance = 0.16f;

    private bool nextStepIsCompensatory;

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
            RunManager.Instance == null ||
            RunManager.Instance.IsRunActive;

        bool blocked =
            (playerSlip != null && playerSlip.IsSlipping) ||
            (playerStun != null && playerStun.IsStunned) ||
            (playerController != null && (playerController.IsFallingSkid || playerController.IsPostSkidImmobile));

        bool moving =
            rb.linearVelocity.sqrMagnitude >
            minimumMovementSpeed * minimumMovementSpeed;

        if (!runActive || blocked || !moving)
        {
            stepTimer = 0f;
            return;
        }

        // Section check: Section 1 = irregular cadence, Section 2 = locked regular cadence
        bool inFirstSection = playerController == null || playerController.IsInFirstSection;
        bool isIrregular = irregularCadence && inFirstSection;

        float baseInterval = walkStepInterval;
        if (playerController != null && playerController.IsCounterBraking)
        {
            // Rapid footsteps while braking and catching balance
            baseInterval = walkStepInterval * 0.52f;
        }
        else if (playerController != null && playerController.IsSprinting)
        {
            baseInterval = sprintStepInterval;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            if (isIrregular)
            {
                // SECTION 1: Irregular cadence & stutter double-steps
                PlayRandomFootstep(pitchVariation: true);

                if (nextStepIsCompensatory)
                {
                    // Compensatory slightly longer pause after a quick stutter
                    stepTimer = Mathf.Max(0.05f, baseInterval * 1.35f);
                    nextStepIsCompensatory = false;
                }
                else if (Random.value < stutterStepChance)
                {
                    // Drunk quick double-step!
                    stepTimer = Mathf.Max(0.05f, baseInterval * 0.45f);
                    nextStepIsCompensatory = true;
                }
                else
                {
                    // Organic timing variance
                    float variance = Random.Range(1f - cadenceVariance, 1f + cadenceVariance);
                    stepTimer = Mathf.Max(0.05f, baseInterval * variance);
                }
            }
            else
            {
                // SECTION 2: Locked-in, rhythmic, rapid footsteps scaling with runaway acceleration
                PlayRandomFootstep(pitchVariation: false);

                float lean = playerController != null ? playerController.LeanProgress : 0.6f;
                float runawayInterval = Mathf.Lerp(walkStepInterval, sprintStepInterval * 0.85f, lean);
                stepTimer = Mathf.Max(0.05f, runawayInterval);
            }
        }
    }

    public void PlayFootstepScuff()
    {
        if (footstepClips != null && footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            PlayClip(clip, footstepVolume * 1.2f);
        }
    }

    private void PlayRandomFootstep(bool pitchVariation = true)
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        AudioClip clip =
            footstepClips[Random.Range(0, footstepClips.Length)];

        // Section 1 has pitch wobble; Section 2 maintains steady pitch
        if (audioSource != null)
        {
            if (pitchVariation)
                audioSource.pitch = Random.Range(0.92f, 1.08f);
            else
            {
                float lean = playerController != null ? playerController.LeanProgress : 0.5f;
                audioSource.pitch = 1.0f + (lean - 0.5f) * 0.15f;
            }
        }

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
