using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DistrictTrigger : MonoBehaviour
{
    [Header("District")]
    [SerializeField] private string districtName = "New District";

    [Header("Optional Music")]
    [SerializeField] private AudioClip districtBGM;

    private BoxCollider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Display the district name.
        DistrictAnnouncementUI.Instance?.ShowDistrict(districtName);

        // Change BGM only if a track is assigned.
        if (districtBGM != null)
        {
            GameBGMManager.Instance?.PlayDistrictMusic(districtBGM);
        }
    }
}