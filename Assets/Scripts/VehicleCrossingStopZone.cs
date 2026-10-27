
using System.Collections.Generic;
using UnityEngine;

public class VehicleCrossingStopZone : MonoBehaviour
{
    [Header("Traffic Light")]
    [SerializeField] private TrafficLightController trafficLight;

    private readonly List<VehicleController> vehiclesInZone =
        new List<VehicleController>();

    private void FixedUpdate()
    {
        for (int i = vehiclesInZone.Count - 1; i >= 0; i--)
        {
            VehicleController vehicle = vehiclesInZone[i];

            if (vehicle == null)
            {
                vehiclesInZone.RemoveAt(i);
                continue;
            }

            if (trafficLight != null && trafficLight.CarsMustStop)
            {
                vehicle.StopAtCrossing();
            }
            else
            {
                vehicle.ContinueDriving();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VehicleController vehicle =
            other.GetComponentInParent<VehicleController>();

        if (vehicle == null)
            return;

        if (!vehiclesInZone.Contains(vehicle))
        {
            vehiclesInZone.Add(vehicle);
        }

        if (trafficLight != null && trafficLight.CarsMustStop)
        {
            vehicle.StopAtCrossing();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        VehicleController vehicle =
            other.GetComponentInParent<VehicleController>();

        if (vehicle == null)
            return;

        vehiclesInZone.Remove(vehicle);
        vehicle.ContinueDriving();
    }

    private void OnDisable()
    {
        foreach (VehicleController vehicle in vehiclesInZone)
        {
            if (vehicle != null)
                vehicle.ContinueDriving();
        }

        vehiclesInZone.Clear();
    }
}
