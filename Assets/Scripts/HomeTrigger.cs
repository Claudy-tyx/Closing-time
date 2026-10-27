using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HomeTrigger : MonoBehaviour
{
    [SerializeField] private ReachedHomeUI victoryUI;

    private bool triggered;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        if (victoryUI == null)
        {
            Debug.LogError("HomeTrigger: Victory UI is missing!");
            return;
        }

        triggered = true;
        victoryUI.ShowVictory();
    }
}