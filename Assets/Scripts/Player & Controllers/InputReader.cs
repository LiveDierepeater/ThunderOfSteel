using UnityEngine;

public abstract class InputReader : MonoBehaviour
{
    protected PlayerEvents PlayerEvents;

    protected virtual void Awake()
    {
        PlayerEvents = new PlayerEvents();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            PlayerEvents.OnLeftMouseButtonPressed?.Invoke(IsModifierPressed());
        
        if (Input.GetKey(KeyCode.Mouse0))
            PlayerEvents.OnLeftMouseButton?.Invoke(IsModifierPressed());
        
        if (Input.GetKeyUp(KeyCode.Mouse0))
            PlayerEvents.OnLeftMouseButtonReleased?.Invoke(IsModifierPressed());
        
        if (Input.GetKeyUp(KeyCode.Mouse1))
            PlayerEvents.OnRightMouseButtonReleased?.Invoke(IsModifierPressed());
    }

    private static bool IsModifierPressed() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
}
