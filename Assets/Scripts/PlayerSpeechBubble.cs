
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerSpeechBubble : MonoBehaviour
{
    [Header("Small Notification Bubble")]
    [SerializeField] private GameObject smallBubble;
    [SerializeField] private TMP_Text smallText;
    [SerializeField] private CanvasGroup smallCanvasGroup;
    [SerializeField] private RectTransform smallRect;

    [Header("Optional Notification Image")]
    [SerializeField] private Image smallItemImage;

    [Header("Large Dialogue Bubble")]
    [SerializeField] private GameObject bigBubble;
    [SerializeField] private TMP_Text bigText;

    [Header("Notification Animation")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float popStartScale = 0.7f;

    [Header("World SFX")]
    [SerializeField] private AudioClip bubblePopClip;
    [SerializeField] private AudioMixerGroup worldSFXMixerGroup;

    [SerializeField, Range(0f, 1f)]
    private float bubblePopVolume = 0.6f;

    private AudioSource worldSFXSource;

    private const int BigCharacterLimit = 90;

    private Coroutine notificationRoutine;

    private readonly Queue<string> dialoguePages =
        new Queue<string>();

    private bool dialogueActive;
    private bool waitingForMouseRelease;
    private Vector3 smallOriginalScale;

    private void Awake()
    {
        worldSFXSource = gameObject.AddComponent<AudioSource>();

        worldSFXSource.playOnAwake = false;
        worldSFXSource.loop = false;
        worldSFXSource.spatialBlend = 0f;
        worldSFXSource.outputAudioMixerGroup = worldSFXMixerGroup;
        smallOriginalScale = smallRect.localScale;

        smallBubble.SetActive(false);
        bigBubble.SetActive(false);

        if (smallItemImage != null)
            smallItemImage.gameObject.SetActive(false);

        smallCanvasGroup.alpha = 0f;
        smallCanvasGroup.blocksRaycasts = false;
        smallCanvasGroup.interactable = false;
    }

    private void PlayBubblePop()
    {
        if (bubblePopClip == null || worldSFXSource == null)
            return;

        worldSFXSource.PlayOneShot(
            bubblePopClip,
            bubblePopVolume
        );
    }

    private void Update()
    {
        if (!dialogueActive || Mouse.current == null)
            return;

        if (waitingForMouseRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
                waitingForMouseRelease = false;

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            NextDialoguePage();
    }

    // SMALL BUBBLE

    public void ShowNotification(string message)
    {
        ShowNotification(message, null);
    }

    public void ShowNotification(string message, Sprite itemSprite)
    {
        bool wasVisible = smallBubble.activeSelf;
        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
            notificationRoutine = null;
        }

        smallText.text = message ?? "";

        if (smallItemImage != null)
        {
            bool hasImage = itemSprite != null;

            smallItemImage.gameObject.SetActive(hasImage);

            if (hasImage)
            {
                smallItemImage.sprite = itemSprite;
                smallItemImage.preserveAspect = true;
            }
        }

        smallBubble.SetActive(true);
        smallCanvasGroup.alpha = 1f;
        if (!wasVisible)
            PlayBubblePop();

        notificationRoutine =
            StartCoroutine(AnimateNotification());
    }

    private IEnumerator AnimateNotification()
    {
        float elapsed = 0f;

        smallRect.localScale =
            smallOriginalScale * popStartScale;

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, popDuration)
            );

            smallRect.localScale = Vector3.Lerp(
                smallOriginalScale * popStartScale,
                smallOriginalScale,
                t
            );

            yield return null;
        }

        smallRect.localScale = smallOriginalScale;

        yield return new WaitForSecondsRealtime(
            displayDuration
        );

        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            smallCanvasGroup.alpha = 1f -
                Mathf.Clamp01(
                    elapsed / Mathf.Max(0.01f, fadeDuration)
                );

            yield return null;
        }

        smallCanvasGroup.alpha = 0f;
        smallBubble.SetActive(false);

        if (smallItemImage != null)
            smallItemImage.gameObject.SetActive(false);

        notificationRoutine = null;
    }

    // BIG BUBBLE

    public void ShowDialogue(string message)
    {
        dialoguePages.Clear();

        foreach (string page in SplitIntoPages(message))
            dialoguePages.Enqueue(page);

        if (dialoguePages.Count == 0)
            return;

        bool wasVisible = bigBubble.activeSelf;

        dialogueActive = true;
        waitingForMouseRelease = true;

        bigBubble.SetActive(true);

        if (!wasVisible)
            PlayBubblePop();

        NextDialoguePage();
    }

    private void NextDialoguePage()
    {
        if (dialoguePages.Count == 0)
        {
            CloseDialogue();
            return;
        }

        bigText.text = dialoguePages.Dequeue();
    }

    public void CloseDialogue()
    {
        dialogueActive = false;
        waitingForMouseRelease = false;
        dialoguePages.Clear();
        bigBubble.SetActive(false);
    }

    private List<string> SplitIntoPages(string message)
    {
        List<string> pages = new List<string>();

        if (string.IsNullOrWhiteSpace(message))
            return pages;

        string[] words = message.Split(' ');
        string currentPage = "";

        foreach (string word in words)
        {
            string candidate = string.IsNullOrEmpty(currentPage)
                ? word
                : currentPage + " " + word;

            if (candidate.Length > BigCharacterLimit &&
                !string.IsNullOrEmpty(currentPage))
            {
                pages.Add(currentPage);
                currentPage = word;
            }
            else
            {
                currentPage = candidate;
            }
        }

        if (!string.IsNullOrEmpty(currentPage))
            pages.Add(currentPage);

        return pages;
    }
}
