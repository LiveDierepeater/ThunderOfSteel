using UnityEngine;

public class UnitCombat : UnitSystem, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack

    public float AttackRange { get; private set; }

#region Internal Fields

    // Private Fields
    private Unit _targetUnit;

#endregion

#region Initializing

    protected override void Awake()
    {
        base.Awake();
        
        AttackRange = Unit.DataUnit.UnitWeaponry.AttackRange;
    }

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.DataUnit.Events.OnNewTargetUnit += SetTarget;
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.DataUnit.Events.OnNewTargetUnit -= SetTarget;
    }

#endregion

#region UPDATES

    private void HandleTick()
    {
        MoveInRange();
        CheckForNewTargetInRange();
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
        
        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            
            // Target is in 'AttackRange'
            if (distanceToTarget <= AttackRange)
                Attack(_targetUnit);
            else
            {
                // Move to target, till Unit is in 'AttackRange'
                
                Unit.DataUnit.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
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

            if ( ! (distance < closestDistance)) continue;
            
            closestDistance = distance;
            closestEnemy = nearbyObject;
        }

        if (closestEnemy is null) return;
        
        if (AttackRange < closestDistance) return;
        
        _targetUnit = closestEnemy.GetComponent<Unit>();
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        // Here Attacking should be implemented
        
        Unit.DataUnit.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
        Unit.DataUnit.Events.OnStopUnit?.Invoke();
        
        Unit.IsAttacking = true; // DEBUG
    }

#endregion
}
