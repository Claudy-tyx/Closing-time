using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

[RequireComponent(typeof(Button))]
public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationSpeed = 12f;

    [Header("Click SFX")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioMixerGroup uiSFXMixerGroup;
    [SerializeField, Range(0f, 1f)]
    private float clickVolume = 0.6f;

    private Button button;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool hovering;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = uiSFXMixerGroup;

        button.onClick.AddListener(PlayClickSound);
    }

    private void Update()
    {
        Vector3 targetScale =
            originalScale * (hovering && button.interactable ? hoverScale : 1f);

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    private void OnDisable()
    {
        hovering = false;
        transform.localScale = originalScale;
    }

    private void PlayClickSound()
    {
        if (clickClip != null)
            audioSource.PlayOneShot(clickClip, clickVolume);
    }
}