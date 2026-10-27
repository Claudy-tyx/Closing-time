
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCDirectionalAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Initial Facing")]
    [SerializeField] private Vector2 initialFacing = Vector2.down;

    [Header("Debug")]
    [SerializeField] private bool logMissingAnimations = true;

    private Vector2 facing;
    private string currentAnimation = "";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        facing = initialFacing.sqrMagnitude > 0.01f
            ? initialFacing.normalized
            : Vector2.down;
    }

    private void Start()
    {
        UpdateAnimation(Vector2.zero);
    }

    public void UpdateAnimation(Vector2 movement)
    {
        bool isMoving = movement.sqrMagnitude > 0.0001f;

        if (isMoving)
            facing = movement.normalized;

        string direction;

        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
            direction = facing.x > 0 ? "Right" : "Left";
        else
            direction = facing.y > 0 ? "Up" : "Down";

        string stateName =
            (isMoving ? "Walk" : "Idle") + direction;

        if (currentAnimation == stateName)
            return;

        int stateHash = Animator.StringToHash(stateName);

        if (!animator.HasState(0, stateHash))
        {
            if (logMissingAnimations)
            {
                Debug.LogWarning(
                    $"[{name}] Missing Animator state: {stateName}",
                    this
                );
            }

            return;
        }

        currentAnimation = stateName;
        animator.Play(stateHash, 0, 0f);
    }
}
