using UnityEngine;

public interface IMovementBehavior
{
    void Initialize(UnitData data, AnimationCurve accelerationCurve, AnimationCurve decelerationCurve);
    
    void MoveToDestination(Vector3 destination);
    
    void StopUnitAtPosition();
}
