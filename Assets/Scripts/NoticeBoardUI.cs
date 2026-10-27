
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class NoticeBoardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button closeButton;
    public bool IsOpen => panel != null && panel.activeSelf;
    [Header("World SFX")]
    [SerializeField] private AudioClip noticeOpenClip;
    [SerializeField] private AudioMixerGroup worldSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float noticeOpenVolume = 0.8f;

    private AudioSource worldSFXSource;

    private void Awake()
    {
        worldSFXSource = gameObject.AddComponent<AudioSource>();

        worldSFXSource.playOnAwake = false;
        worldSFXSource.loop = false;
        worldSFXSource.spatialBlend = 0f;
        worldSFXSource.outputAudioMixerGroup = worldSFXMixerGroup;

        panel.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    private void PlayWorldSFX(AudioClip clip, float volume)
    {
        if (clip == null || worldSFXSource == null)
            return;

        worldSFXSource.PlayOneShot(clip, volume);
    }

    public void Open(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;

        panel.SetActive(true);
        RunConsciousnessTimer.Pause(this);
        PlayWorldSFX(noticeOpenClip, noticeOpenVolume);
    }

    public void Close()
    {
        RunConsciousnessTimer.Resume(this);
        panel.SetActive(false);
    }

    private void OnDisable()
    {
        RunConsciousnessTimer.Resume(this);
    }

    private void OnDestroy()
    {
        RunConsciousnessTimer.Resume(this);
    }
}
