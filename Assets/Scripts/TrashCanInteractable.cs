
using UnityEngine;
using UnityEngine.Audio;

public class TrashCanInteractable : InteractableObject
{
    [System.Serializable]
    public class TrashItem
    {
        public string itemName;
        public Sprite itemSprite;

        [Min(0)]
        public int coinValue = 2;
    }

    [Header("Random Trash Items")]
    [SerializeField] private TrashItem[] possibleItems;

    [Header("Reward Chances (%)")]
    [SerializeField] private float nothingChance = 40f;
    [SerializeField] private float itemChance = 55f;

    [Header("Trash Can Visuals")]
    [SerializeField] private SpriteRenderer trashCanRenderer;
    [SerializeField] private Sprite emptyTrashCanSprite;

    [Tooltip("Optional visual effect to disable after searching.")]
    [SerializeField] private GameObject searchVisualEffect;

    [Header("Run Data")]
    [SerializeField] private RunData runData;

    [Header("Audio")]
    [SerializeField] private AudioClip rummageClip;
    [SerializeField, Range(0f, 1f)] private float rummageVolume = 0.8f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private bool searched;
    private PlayerSpeechBubble speechBubble;

    private void Start()
    {
        if (runData == null && RunManager.Instance != null)
            runData = RunManager.Instance.Data;

        if (trashCanRenderer == null)
            trashCanRenderer = GetComponent<SpriteRenderer>();

        speechBubble =
            FindFirstObjectByType<PlayerSpeechBubble>();
    }

    protected override void Interact()
    {
        if (searched)
        {
            ShowNotification("It's empty...");
            return;
        }

        if (runData == null)
        {
            Debug.LogWarning("TrashCan: RunData missing!");
            return;
        }

        searched = true;
        UpdateSearchedVisuals();

        if (rummageClip != null)
        {
            GameObject soundObject = new GameObject("TrashRummageSFX");

            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = rummageClip;
            source.volume = rummageVolume;
            source.spatialBlend = 0f; // 2D audio
            source.outputAudioMixerGroup = sfxMixerGroup;

            source.Play();

            Destroy(soundObject, rummageClip.length + 0.1f);
        }

        float roll = Random.Range(0f, 100f);

        if (roll < nothingChance)
        {
            ShowNotification("Nothing in here");
        }
        else if (roll < nothingChance + itemChance)
        {
            GiveRandomItem();
        }
        else
        {
            if (!GameSaveData.HasBusPass)
            {
                GameSaveData.UnlockBusPass();
                ShowNotification("Wow! A bus pass!");
            }
            else
            {
                ShowNotification("Another bus pass?");
            }
        }
    }

    
    private void GiveRandomItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
        {
            ShowNotification("Nothing in here");
            return;
        }

        TrashItem item = possibleItems[
            Random.Range(0, possibleItems.Length)
        ];

        if (item == null)
        {
            ShowNotification("Nothing in here");
            return;
        }

        runData.CollectTrashItem(
            item.itemName,
            item.itemSprite,
            item.coinValue
        );

        string message = $"Found {item.itemName}!";

        if (speechBubble == null)
            speechBubble =
                FindFirstObjectByType<PlayerSpeechBubble>();

        if (speechBubble != null)
            speechBubble.ShowNotification(message, item.itemSprite);
        else
            Debug.Log(message);
    }


    private void UpdateSearchedVisuals()
    {
        if (trashCanRenderer != null &&
            emptyTrashCanSprite != null)
        {
            trashCanRenderer.sprite = emptyTrashCanSprite;
        }

        if (searchVisualEffect != null)
            searchVisualEffect.SetActive(false);
    }

    private void ShowNotification(string message)
    {
        if (speechBubble == null)
        {
            speechBubble =
                FindFirstObjectByType<PlayerSpeechBubble>();
        }

        if (speechBubble != null)
            speechBubble.ShowNotification(message);
        else
            Debug.Log(message);
    }
}
