using UnityEngine;

public interface IMovementBehavior
{
    void Initialize(UnitData data, AnimationCurve accelerationCurve, AnimationCurve decelerationCurve);
    void MoveToDestination(Vector3 destination);

    void CalculateNewPath(Vector3 destination);

    void CalculateNewDestinationToAttack(Unit targetUnit, float attackRange);
}
