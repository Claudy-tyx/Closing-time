
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class RunConsciousnessTimer : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("Consciousness points lost per second.")]
    [SerializeField] private float consciousnessLossPerSecond = 1f;

    [Header("Scene Settings")]
    [Tooltip("Only this scene will drain consciousness.")]
    [SerializeField] private string citySceneName = "NewCity";

    [Header("Low Consciousness Warning SFX")]
    [SerializeField] private AudioClip lowTimerClip;
    [SerializeField] private AudioMixerGroup worldSFXMixerGroup;

    [SerializeField] private float warningThreshold = 20f;

    [SerializeField, Range(0f, 1f)]
    private float warningVolume = 0.7f;

    private AudioSource warningSource;

    private RunData runData;

    private bool warningPaused;

    // Each UI or transition can pause the timer independently.
    private static readonly HashSet<object> pauseSources =
        new HashSet<object>();

    public static bool IsPaused => pauseSources.Count > 0;

    public static void Pause(object source)
    {
        if (source == null)
            return;

        pauseSources.Add(source);
    }

    public static void Resume(object source)
    {
        if (source == null)
            return;

        pauseSources.Remove(source);
    }

    private void Awake()
    {
        warningSource = gameObject.AddComponent<AudioSource>();

        warningSource.clip = lowTimerClip;
        warningSource.outputAudioMixerGroup = worldSFXMixerGroup;
        warningSource.volume = warningVolume;

        warningSource.playOnAwake = false;
        warningSource.loop = true;
        warningSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;
    }

    private void Update()
    {
        if (RunManager.Instance == null ||
            !RunManager.Instance.IsRunActive ||
            SceneManager.GetActiveScene().name != citySceneName)
        {
            UpdateWarningSound();
            return;
        }

        if (runData == null)
            runData = RunManager.Instance.Data;

        if (runData == null)
        {
            UpdateWarningSound();
            return;
        }

        if (!IsPaused && runData.CurrentConsciousness > 0f)
        {
            runData.ChangeConsciousness(
                -consciousnessLossPerSecond * Time.deltaTime
            );
        }

        UpdateWarningSound();
    }

    private void UpdateWarningSound()
    {
        if (warningSource == null || lowTimerClip == null)
            return;

        bool shouldPlay =
            RunManager.Instance != null &&
            RunManager.Instance.IsRunActive &&
            SceneManager.GetActiveScene().name == citySceneName &&
            runData != null &&
            runData.CurrentConsciousness > 0f &&
            runData.CurrentConsciousness <= warningThreshold;

        if (!shouldPlay)
        {
            warningSource.Stop();
            warningPaused = false;
            return;
        }
        if (IsPaused)
        {
            if (!warningPaused && warningSource.isPlaying)
            {
                warningSource.Pause();
                warningPaused = true;
            }

            return;
        }

        if (warningPaused)
        {
            warningSource.UnPause();
            warningPaused = false;
        }
        else if (!warningSource.isPlaying)
        {
            warningSource.Play();
        }
    }

    private void OnDisable()
    {
        if (warningSource != null)
            warningSource.Stop();

        warningPaused = false;
    }
}
