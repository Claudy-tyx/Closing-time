
using UnityEngine;

public class GamePerformanceSettings : MonoBehaviour
{
    [Header("Performance")]
    [SerializeField] private int targetFPS = 60;

    private void Awake()
    {
        // Disable VSync so targetFrameRate can control FPS.
        QualitySettings.vSyncCount = 0;

        // Limit the game's frame rate.
        Application.targetFrameRate = targetFPS;
    }
}
