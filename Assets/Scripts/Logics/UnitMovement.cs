using UnityEngine;

public abstract class UnitMovement : MonoBehaviour, IMovementBehavior
{
    public abstract void MoveToDestination(Vector3 destination);
}
