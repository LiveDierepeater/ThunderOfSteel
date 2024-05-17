using System;
using UnityEngine;

public struct UnitEvents
{
    //public delegate void UnitIsInRange(Vector3 newDestination);
    public Action<Vector3> OnAttackUnit;
    public Action<Unit> OnNewTargetUnit;
    public Action OnStopUnit;
    public Action<Vector3, int> OnAttack;
    public Action OnUnitDeath;
    public Action <Vector3> OnUnitFlee;
    public Action OnUnitOperational;
}
