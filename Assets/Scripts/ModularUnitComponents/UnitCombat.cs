using UnityEngine;

public class UnitCombat : UnitSystem, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack

    public float MaxAttackRange { get; private set; }

#region Internal Fields

    // Private Fields
    private Unit _targetUnit;

#endregion

#region Initializing

    protected override void Awake()
    {
        base.Awake();

        MaxAttackRange = GetMaxAttackRange();
    }

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit += SetTarget;
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit -= SetTarget;
    }

#endregion

#region UPDATES

    private void HandleTick()
    {
        switch (_targetUnit)
        {
            case not null:  // Unit has target
                MoveInRange();
                break;
            
            case null:      // Unit has NO target
                CheckForNewTargetInRange();
                break;
        }
    }

#endregion

#region External Called Logic

    public void SetTarget(Unit target)
    {
        _targetUnit = target;
    }

#endregion

#region Intern Logic

    private void MoveInRange()
    {
        Unit.IsAttacking = false; // DEBUG
        
        if (Unit.UnitData.CurrentUnitCommand != UnitData.UnitCommands.Attack) return;
        
        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            
            // Target is in 'AttackRange'
            if (distanceToTarget <= MaxAttackRange)
                Attack(_targetUnit);
            else
            {
                // Move to target, till Unit is in 'AttackRange'
                
                Unit.UnitData.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
                Unit.IsAttacking = false; // DEBUG
            }
        }
    }

    private void CheckForNewTargetInRange()
    {
        if (_targetUnit is not null) return;

        var nearbyObjects = SpatialHashManager.Instance.SpatialHash.GetNearbyUnitObjectsInNearbyHashKeys(transform.position);
        GameObject closestEnemy = null;
        var closestDistance = 1000f;

        foreach (var nearbyObject in nearbyObjects)
        {
            var distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
            
            if (nearbyObject == gameObject) continue; // Continue, when nearby Object is 'this.gameObject'
            
            if ( ! (distance < closestDistance)) continue;
            
            closestDistance = distance;
            closestEnemy = nearbyObject;
        }

        if (closestEnemy is null) return;
        
        if (MaxAttackRange < closestDistance) return;
        
        _targetUnit = closestEnemy.GetComponent<Unit>();
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        // Here Attacking should be implemented
        
        Unit.UnitData.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
        Unit.UnitData.Events.OnStopUnit?.Invoke();
        
        Unit.IsAttacking = true; // DEBUG
    }

#endregion

#region Extracted Return Methods

private float GetMaxAttackRange()   // Returns the maximal 'attackRange' out of the multiple weapons an Unit can have
{
    float currentAttackRange = 0;
        
    foreach (var weaponry in Unit.UnitData.UnitWeaponry)
    {
        if (weaponry.AttackRange > currentAttackRange)
            currentAttackRange = weaponry.AttackRange;
    }

    return currentAttackRange;
}

#endregion
}
