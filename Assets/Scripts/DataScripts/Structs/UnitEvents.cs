using System;
using UnityEngine;

public struct UnitEvents
{
    public Action<Vector3> OnCommandToDestination;
    public Action<Unit> OnCommandToAttack;
    public Action<Unit> OnNewTargetUnit;
    public Action<Vector3, int> OnAttack;
    public Action OnUnitDeath;
    public Action OnHandleUnitDeathForSpotting;
    public Action <Vector3> OnUnitFlee;
    public Action OnUnitOperational;
    
    public delegate bool CheckForEnemyUnits(int unitID);
    public CheckForEnemyUnits OnCheckForEnemyUnit;

    public delegate float GetMaxAttackRange();
    public GetMaxAttackRange OnGetMaxAttackRange;
}
