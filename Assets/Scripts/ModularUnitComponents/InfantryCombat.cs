using UnityEngine;

public class InfantryCombat : UnitSystem, IAttackBehavior
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
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
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
        Unit.IsAttacking = false; // DEBUG
        
        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            
            // Target is in 'AttackRange'
            if (distanceToTarget <= AttackRange)
            {
                Attack(_targetUnit);
            }
            else
            {
                // Move to target, till Unit is in 'AttackRange'
                
                Unit.DataUnit.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
                //Unit.CommandToDestination(_targetUnit.transform.position);
                Unit.IsAttacking = false; // DEBUG
            }
        }
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        // Here Attacking should be implemented

        // DEBUG
        Debug.DrawLine(transform.position, _targetUnit.transform.position, Color.magenta, 3f);
        print(Vector3.Distance(transform.position, _targetUnit.transform.position) + " | ID: " + GetInstanceID());
        
        Unit.DataUnit.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
        Unit.DataUnit.Events.OnStopUnit?.Invoke();
        //Unit.StopUnit();
        Unit.IsAttacking = true; // DEBUG
    }

#endregion
}
