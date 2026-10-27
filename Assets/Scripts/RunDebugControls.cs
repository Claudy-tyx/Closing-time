
using UnityEngine;
using UnityEngine.InputSystem;

public class RunDebugControls : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null ||
            RunManager.Instance == null)
            return;

        // F6: Start a run
        if (Keyboard.current.f6Key.wasPressedThisFrame)
            RunManager.Instance.BeginNewRun();

        // F7: End a run and convert trash to coins
        if (Keyboard.current.f7Key.wasPressedThisFrame)
            RunManager.Instance.EndRun();
    }
}
