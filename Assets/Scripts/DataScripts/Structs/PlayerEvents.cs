using System;

public struct PlayerEvents
{
    public Action<bool> OnLeftMouseButtonPressed;
    public Action<bool> OnLeftMouseButton;
    public Action<bool> OnLeftMouseButtonReleased;
    
    public Action<bool> OnRightMouseButtonReleased;
}
