
using UnityEngine;

public class NoticeBoardInteractable : InteractableObject
{
    [Header("Notice Content")]
    [SerializeField] private string noticeTitle = "Notice Board";

    [TextArea(4, 10)]
    [SerializeField] private string noticeDescription =
        "Welcome to the neighbourhood!";

    private NoticeBoardUI noticeUI;
    private bool openedByThisBoard;

    private static NoticeBoardInteractable activeBoard;

    private void Start()
    {
        noticeUI = FindAnyObjectByType<NoticeBoardUI>();
    }

    protected override void Interact()
    {
        if (noticeUI == null)
        {
            Debug.LogWarning("NoticeBoardUI not found!");
            return;
        }

        if (activeBoard != null && activeBoard != this)
            activeBoard.CloseNotice();

        activeBoard = this;
        openedByThisBoard = true;

        noticeUI.Open(noticeTitle, noticeDescription);
    }

    protected override void Update()
    {
        base.Update();

        if (!openedByThisBoard)
            return;

        // Close when player walks away or UI closes manually.
        if (!IsPlayerInRange ||
            noticeUI == null ||
            !noticeUI.IsOpen)
        {
            CloseNotice();
        }
    }

    private void CloseNotice()
    {
        if (!openedByThisBoard)
            return;

        openedByThisBoard = false;

        if (activeBoard == this)
            activeBoard = null;

        if (noticeUI != null)
            noticeUI.Close();
    }

    protected override void OnDisable()
    {
        CloseNotice();
        base.OnDisable();
    }
}
