
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogueUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    private PlayerController player;
    private string[] lines;
    private int currentLine;
    private Action onDialogueFinished;

    public static bool IsDialogueActive { get; private set; }

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextLine);
    }

    public bool ShowDialogue(
        string npcName,
        string[] dialogueLines,
        Action onFinished = null)
    {
        if (IsDialogueActive ||
            BartenderDialogueUI.IsDialogueActive ||
            dialoguePanel == null ||
            dialogueText == null ||
            dialogueLines == null ||
            dialogueLines.Length == 0)
            return false;

        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
            player.SetMovementLocked(true);

        lines = dialogueLines;
        currentLine = 0;
        onDialogueFinished = onFinished;

        IsDialogueActive = true;

        if (speakerName != null)
            speakerName.text = npcName;

        dialoguePanel.SetActive(true);
        DisplayCurrentLine();

        return true;
    }

    private void DisplayCurrentLine()
    {
        dialogueText.text = lines[currentLine];

        if (nextButtonText != null)
        {
            nextButtonText.text =
                currentLine == lines.Length - 1
                    ? "Close"
                    : "Continue";
        }
    }

    public void NextLine()
    {
        if (!IsDialogueActive)
            return;

        currentLine++;

        if (currentLine >= lines.Length)
        {
            CloseDialogue();
            return;
        }

        DisplayCurrentLine();
    }

    public void CloseDialogue()
    {
        if (!IsDialogueActive)
            return;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        IsDialogueActive = false;

        if (player != null)
            player.SetMovementLocked(false);

        player = null;

        Action callback = onDialogueFinished;
        onDialogueFinished = null;
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextLine);

        if (IsDialogueActive)
        {
            IsDialogueActive = false;

            if (player != null)
                player.SetMovementLocked(false);
        }
    }
}
