
using System.Collections;
using UnityEngine;

public class AnimatedDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Animation States")]
    [SerializeField] private string openingState = "DoorOpen";
    [SerializeField] private string closingState = "DoorClose";

    [Header("Animation Durations")]
    [SerializeField] private float openingDuration = 0.4f;
    [SerializeField] private float closingDuration = 0.4f;

    private Coroutine routine;
    private bool playerNearby;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Start with the door fully closed.
        PlayAnimationAtStart(openingState);
    }

    public void SetPlayerNearby(bool nearby)
    {
        if (playerNearby == nearby)
            return;

        playerNearby = nearby;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(
            nearby ? OpenDoor() : CloseDoor()
        );
    }

    private IEnumerator OpenDoor()
    {
        IsOpen = false;

        animator.speed = 1f;
        animator.Play(openingState, 0, 0f);

        yield return new WaitForSeconds(
            Mathf.Max(0.01f, openingDuration)
        );

        if (playerNearby)
        {
            // Freeze on the final open frame.
            animator.Play(openingState, 0, 1f);
            animator.Update(0f);
            animator.speed = 0f;

            IsOpen = true;
        }

        routine = null;
    }

    private IEnumerator CloseDoor()
    {
        IsOpen = false;

        animator.speed = 1f;
        animator.Play(closingState, 0, 0f);

        yield return new WaitForSeconds(
            Mathf.Max(0.01f, closingDuration)
        );

        if (!playerNearby)
        {
            // Freeze on the final closed frame.
            animator.Play(closingState, 0, 1f);
            animator.Update(0f);
            animator.speed = 0f;
        }

        routine = null;
    }

    private void PlayAnimationAtStart(string stateName)
    {
        animator.speed = 0f;
        animator.Play(stateName, 0, 0f);
        animator.Update(0f);

        IsOpen = false;
    }
}
