using UnityEngine;

public class VehicleDespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        VehicleController vehicle =
            other.GetComponent<VehicleController>();

        if (vehicle != null)
        {
            Destroy(vehicle.gameObject);
        }
    }
}