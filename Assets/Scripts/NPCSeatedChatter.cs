
using System.Collections;
using UnityEngine;

public class NPCSeatedChatter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCSpeechBubble speechBubble;
    [SerializeField] private NPCBumpReaction bumpReaction;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Sitting Animation")]
    [SerializeField] private string sittingAnimationName = "SitIdle";

    [Header("Custom Messages")]
    [TextArea(1, 2)]
    [SerializeField] private string[] messages =
    {
        "So relaxing...",
        "Nice weather!",
        "I'm resting",
        "Need a break"
    };

    [Header("Chatter Timing")]
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 20f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 7f;

    [Header("Collision Chatter Pause")]
    [SerializeField] private float chatterPauseAfterBump = 3f;

    private float nextAllowedChatterTime;

    private void Awake()
    {
        if (speechBubble == null)
            speechBubble =
                GetComponentInChildren<NPCSpeechBubble>(true);

        if (bumpReaction == null)
            bumpReaction = GetComponent<NPCBumpReaction>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        FindPlayer();
        PlaySittingAnimation();
        StartCoroutine(ChatterLoop());
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void PlaySittingAnimation()
    {
        if (animator == null)
            return;

        int stateHash =
            Animator.StringToHash(sittingAnimationName);

        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
        }
        else
        {
            Debug.LogWarning(
                $"[{name}] Missing sitting animation state: " +
                sittingAnimationName,
                this
            );
        }
    }

    private IEnumerator ChatterLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(
                Mathf.Max(0.1f, minInterval),
                Mathf.Max(minInterval, maxInterval)
            );

            yield return new WaitForSeconds(waitTime);

            if (speechBubble == null)
                continue;

            if (messages == null || messages.Length == 0)
                continue;

            if (player == null)
            {
                FindPlayer();

                if (player == null)
                    continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                player.position
            );

            if (distance > detectionRadius)
                continue;

            // Don't interrupt the NPC's collision reaction.
            if (bumpReaction != null && bumpReaction.IsBumping)
            {
                nextAllowedChatterTime =
                    Time.time + chatterPauseAfterBump;

                continue;
            }

            if (Time.time < nextAllowedChatterTime)
                continue;

            string message =
                messages[Random.Range(0, messages.Length)];

            if (string.IsNullOrWhiteSpace(message))
                continue;

            speechBubble.Show(message);
        }
    }
}
