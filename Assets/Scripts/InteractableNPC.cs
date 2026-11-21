
using UnityEngine;

public class InteractableNPC : InteractableObject
{
    [Header("NPC Information")]
    [SerializeField] private string npcName = "Villager";

    [Header("Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] dialogueLines =
    {
        "Hello there!",
        "Be careful on your way home."
    };

    [Header("Dialogue UI")]
    [SerializeField] private NPCDialogueUI dialogueUI;

    protected override void Interact()
    {
        if (NPCDialogueUI.IsDialogueActive ||
            BartenderDialogueUI.IsDialogueActive)
            return;

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<NPCDialogueUI>();

        if (dialogueUI == null)
        {
            Debug.LogWarning(
                $"NPC {npcName}: NPCDialogueUI not found."
            );
            return;
        }

        if (dialogueLines == null ||
            dialogueLines.Length == 0)
            return;

        dialogueUI.ShowDialogue(npcName, dialogueLines);
    }
}
