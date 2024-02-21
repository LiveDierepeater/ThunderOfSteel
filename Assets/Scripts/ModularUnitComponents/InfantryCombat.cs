using UnityEngine;

public class InfantryCombat : MonoBehaviour, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack

    public float AttackRange { get; private set; }

#region Internal Fields

    // Private Fields
    private Unit _unit;
    private Unit _targetUnit;

#endregion

#region Initializing

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
    }

    public void Initialize(UnitWeaponry data)
    {
        // Passing the UnitData in
        _unit = GetComponent<Unit>();

        AttackRange = data.AttackRange;
    }

#endregion

#region UPDATES

    private void HandleTick()
    {
        MoveInRange();
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
        _unit.IsAttacking = false; // DEBUG
        
        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            
            // Target is in 'AttackRange'
            if (distanceToTarget <= AttackRange)
                Attack(_targetUnit);
            else
            {
                // Move to target, till Unit is in 'AttackRange'
                
                _unit.CommandToDestination(_targetUnit.transform.position);
                _unit.IsAttacking = false; // DEBUG
            }
        }
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        // Here Attacking should be implemented
    
        _unit.StopUnit();
        _unit.IsAttacking = true; // DEBUG
    }

#endregion
}
