using System.Collections;
using TMPro;
using UnityEngine;

public class DistrictAnnouncementUI : MonoBehaviour
{
    public static DistrictAnnouncementUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text districtNameText;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private Coroutine animationRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowDistrict(string districtName)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        districtNameText.text = districtName;

        animationRoutine = StartCoroutine(AnimateAnnouncement());
    }

    private IEnumerator AnimateAnnouncement()
    {
        float elapsed = 0f;

        // Fade in.
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeInDuration));

            yield return null;
        }

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;

        // Fade out.
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha =
                1f - Mathf.Clamp01(
                    elapsed / Mathf.Max(0.01f, fadeOutDuration)
                );

            yield return null;
        }

        canvasGroup.alpha = 0f;
        animationRoutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}