
using UnityEngine;

public class PedestrianCrossing : MonoBehaviour
{
    [Header("Traffic Light")]
    [SerializeField] private TrafficLightController trafficLight;

    public TrafficLightController TrafficLight => trafficLight;
}
