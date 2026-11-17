using System.Collections;
using UnityEngine;

public class BartenderNPC : InteractableObject
{
    [Header("References")]
    [SerializeField] private BartenderDialogueUI dialogueUI;
    [SerializeField] private PlayerStatsUI playerStatsUI;

    [Header("General Hints")]
    [TextArea(2, 4)]
    [SerializeField] private string[] generalHints =
    {
        "Keep an eye on your consciousness. Once it's gone, you're done for.",
        "Upgrades might seem expensive, but they'll save you a lot of trouble.",
        "Don't rush across the road. Those drivers aren't slowing down for you.",
        "Sometimes the longer route is the safer one.",
        "If you're struggling, try improving your balance first."
    };

    [Header("Map")]
    [SerializeField] private PubMapUI pubMapUI;

    private int nextHintIndex;
    private bool automaticDialogueStarted;
    private Coroutine tutorialRoutine;

    private IEnumerator Start()
    {
        yield return null;
        TryStartMilestoneDialogue();
    }

    protected override void Interact()
    {
        if (dialogueUI == null ||
            BartenderDialogueUI.IsDialogueActive)
            return;

        if (generalHints == null || generalHints.Length == 0)
            return;

        string hint = generalHints[nextHintIndex];

        nextHintIndex =
            (nextHintIndex + 1) % generalHints.Length;

        dialogueUI.ShowDialogue(new string[] { hint });
    }

    private void TryStartMilestoneDialogue()
    {
        RunManager manager = RunManager.Instance;

        if (automaticDialogueStarted ||
            dialogueUI == null ||
            manager == null)
            return;

        int failures = manager.TotalFailedRuns;

        if (failures >= 1 &&
            !manager.FirstUpgradeTutorialCompleted)
        {
            StartMilestone(
                new string[]
                {
                    "Back already? That didn't take long.",
                    "You won't get very far if you keep passing out.",
                    "See that distance you've been walking? You can spend it on upgrades.",
                    "Here, I'll give you enough to buy your first Consciousness upgrade.",
                    "Open your upgrades and follow the arrow."
                },
                StartConsciousnessTutorial
            );
        }
        else if (failures >= 3 &&
                 !manager.ThirdFailureRewardClaimed)
        {
            StartMilestone(
                new string[]
                {
                    "Running short on cash?",
                    "You'd be surprised what people throw away around here.",
                    "Try checking the trash cans. You might find something valuable.",
                    "Here, take these 10 coins. Consider it a little encouragement!"
                },
                manager.ClaimThirdFailureReward
            );
        }
        else if (failures >= 5 &&
                 !manager.FifthFailureRewardClaimed)
        {
            StartMilestone(
                new string[]
                {
                    "You look terrible. Have you eaten anything?",
                    "The shops around town sell food and drinks.",
                    "Some can restore your hearts or consciousness.",
                    "Here, take 15 coins. Get yourself something useful!"
                },
                manager.ClaimFifthFailureReward
            );
        }
        else if (failures >= 10 &&
         !manager.PubMapUnlocked)
        {
            StartMilestone(
                new string[]
                {
                    "Ten failed attempts? Seriously?",
                    "Alright, I think you need a little more help.",
                    "Here, take this map of the city.",
                    "You can open it whenever you're back at the pub.",
                    "Maybe now you'll finally find your way home!"
                },
                UnlockPubMap
            );
        }
    }

    private void StartMilestone(
        string[] lines,
        System.Action onFinished = null)
    {
        automaticDialogueStarted = true;
        dialogueUI.ShowDialogue(lines, onFinished);
    }

    private void StartConsciousnessTutorial()
    {
        if (tutorialRoutine != null)
            StopCoroutine(tutorialRoutine);

        tutorialRoutine =
            StartCoroutine(ConsciousnessTutorialRoutine());
    }

    private IEnumerator ConsciousnessTutorialRoutine()
    {
        RunManager manager = RunManager.Instance;

        if (manager == null || manager.Data == null ||
            playerStatsUI == null)
            yield break;

        RunData data = manager.Data;

        int startingPurchases = data.ConsciousnessPurchases;

        // Give enough currency for the next Consciousness upgrade.
        int needed = Mathf.Max(
            0,
            data.GetConsciousnessCost() - data.DistanceAvailable
        );

        data.GrantDistanceCurrency(needed);

        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        if (player != null)
            player.SetMovementLocked(true);

        playerStatsUI.Open();
        playerStatsUI.SetConsciousnessTutorialActive(true);

        // Wait until the player purchases Consciousness.
        yield return new WaitUntil(
            () => data.ConsciousnessPurchases > startingPurchases
        );

        playerStatsUI.SetConsciousnessTutorialActive(false);

        manager.CompleteFirstUpgradeTutorial();

        if (player != null)
            player.SetMovementLocked(false);

        tutorialRoutine = null;
    }

    private void OnDestroy()
    {
        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
            tutorialRoutine = null;
        }

        if (playerStatsUI != null)
            playerStatsUI.SetConsciousnessTutorialActive(false);

        PlayerController player =
            FindFirstObjectByType<PlayerController>();

        if (player != null)
            player.SetMovementLocked(false);
    }

    private void UnlockPubMap()
    {
        RunManager manager = RunManager.Instance;

        if (manager == null)
            return;

        manager.UnlockPubMap();

        if (pubMapUI != null)
            pubMapUI.RefreshUnlock();

        Debug.Log("Bartender gifted the city map!");
    }
    }