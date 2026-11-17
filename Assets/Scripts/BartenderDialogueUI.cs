using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BartenderDialogueUI : MonoBehaviour
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

    public void ShowDialogue(
        string[] dialogueLines,
        Action onFinished = null)
    {
        if (IsDialogueActive ||
            dialoguePanel == null ||
            dialogueText == null ||
            dialogueLines == null ||
            dialogueLines.Length == 0)
            return;

        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
            player.SetMovementLocked(true);

        lines = dialogueLines;
        currentLine = 0;
        onDialogueFinished = onFinished;

        IsDialogueActive = true;

        if (speakerName != null)
            speakerName.text = "Bartender";

        dialoguePanel.SetActive(true);
        DisplayCurrentLine();
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

    private void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        IsDialogueActive = false;

        if (player != null)
            player.SetMovementLocked(false);

        Action callback = onDialogueFinished;
        onDialogueFinished = null;
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (IsDialogueActive)
        {
            IsDialogueActive = false;

            if (player != null)
                player.SetMovementLocked(false);
        }
    }
}