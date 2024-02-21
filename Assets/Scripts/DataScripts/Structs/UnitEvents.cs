using System;
using UnityEngine;

public struct UnitEvents
{
    //public delegate void UnitIsInRange(Vector3 newDestination);
    public Action<Vector3> OnAttackUnit;

    public Action OnStopUnit;
}
