
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Heart Layout")]
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private Vector2 heartSize =
        new Vector2(32f, 32f);

    private RunData runData;

    private readonly List<Image> heartImages =
        new List<Image>();

    private void OnEnable()
    {
        if (RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (runData != null)
            runData.OnDataChanged += RefreshHearts;

        RefreshHearts();
    }

    private void OnDisable()
    {
        if (runData != null)
            runData.OnDataChanged -= RefreshHearts;
    }

    private void RefreshHearts()
    {
        if (runData == null || heartsContainer == null)
            return;

        int maxHearts = runData.MaxHearts;
        int currentHearts = runData.CurrentHearts;

        // Add missing heart images.
        while (heartImages.Count < maxHearts)
        {
            GameObject heartObject =
                new GameObject("Heart", typeof(RectTransform));

            heartObject.transform.SetParent(
                heartsContainer,
                false
            );

            Image image = heartObject.AddComponent<Image>();
            image.preserveAspect = true;

            RectTransform rect =
                heartObject.GetComponent<RectTransform>();

            rect.sizeDelta = heartSize;

            heartImages.Add(image);
        }

        // Update heart sprites.
        for (int i = 0; i < heartImages.Count; i++)
        {
            bool shouldShow = i < maxHearts;

            heartImages[i].gameObject.SetActive(shouldShow);

            if (!shouldShow)
                continue;

            heartImages[i].sprite =
                i < currentHearts
                    ? fullHeartSprite
                    : emptyHeartSprite;
        }
    }
}
