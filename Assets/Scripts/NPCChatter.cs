
using System.Collections;
using UnityEngine;

public class NPCChatter : MonoBehaviour
{
    public enum IdleDirection
    {
        Down,
        Up,
        Left,
        Right
    }

    [Header("References")]
    [SerializeField] private NPCSpeechBubble speechBubble;
    [SerializeField] private NPCBumpReaction bumpReaction;
    [SerializeField] private NPCDirectionalAnimator npcAnimator;
    [SerializeField] private Transform player;

    [Header("Idle Animation")]
    [SerializeField] private IdleDirection idleDirection =
        IdleDirection.Down;

    [Header("Custom Messages")]
    [TextArea(1, 2)]
    [SerializeField] private string[] messages =
    {
        "Nice weather!",
        "I'm so tired",
        "Need coffee...",
        "What a day!"
    };

    [Header("Chatter Timing")]
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 20f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 7f;

    [Header("Collision Chatter Pause")]
    [SerializeField] private float chatterPauseAfterBump = 3f;

    private float nextAllowedChatterTime;
    private Animator animator;

    private void Awake()
    {
        if (speechBubble == null)
            speechBubble =
                GetComponentInChildren<NPCSpeechBubble>(true);

        if (bumpReaction == null)
            bumpReaction = GetComponent<NPCBumpReaction>();

        if (npcAnimator == null)
            npcAnimator = GetComponent<NPCDirectionalAnimator>();

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        FindPlayer();
        ApplyIdleDirection();
        StartCoroutine(ChatterLoop());
    }

    private void ApplyIdleDirection()
    {
        if (animator == null)
            return;

        string stateName = "Idle" + idleDirection.ToString();

        int stateHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
        }
        else
        {
            Debug.LogWarning(
                $"[{name}] Missing animation state: {stateName}",
                this
            );
        }
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
