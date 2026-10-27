using System.Collections.Generic;
using UnityEngine;

public class VehicleFrontSensor : MonoBehaviour
{
    [SerializeField] private VehicleController myVehicle;

    private readonly HashSet<VehicleController> detectedVehicles =
        new HashSet<VehicleController>();

    private void Awake()
    {
        if (myVehicle == null)
            myVehicle = GetComponentInParent<VehicleController>();
    }

    public bool HasQueuedVehicleAhead
    {
        get
        {
            if (myVehicle == null)
                return false;

            detectedVehicles.RemoveWhere(vehicle => vehicle == null);

            foreach (VehicleController vehicle in detectedVehicles)
            {
                if (vehicle == myVehicle)
                    continue;

                // Only consider vehicles moving in the same direction
                if (vehicle.CurrentDirection != myVehicle.CurrentDirection)
                    continue;

                // Only consider vehicles physically ahead
                Vector2 difference =
                    vehicle.transform.position - myVehicle.transform.position;

                float forwardDistance =
                    Vector2.Dot(difference, myVehicle.ForwardDirection);

                if (forwardDistance <= 0f)
                    continue;

                // Only queue behind stopped vehicles
                if (vehicle.IsWaitingInQueue)
                    return true;
            }

            return false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VehicleController vehicle =
            other.GetComponentInParent<VehicleController>();

        if (vehicle == null || vehicle == myVehicle)
            return;

        detectedVehicles.Add(vehicle);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        VehicleController vehicle =
            other.GetComponentInParent<VehicleController>();

        if (vehicle == null)
            return;

        detectedVehicles.Remove(vehicle);
    }
}