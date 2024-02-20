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
        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            if (distanceToTarget <= AttackRange)
                // Ziel ist in Angriffsreichweite
                Attack(_targetUnit);
            else
                // Bewege dich zum Ziel, bis du in Angriffsreichweite bist
                _unit.CommandToDestination(_targetUnit.transform.position);
        }
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit target)
    {
        // Implementiere den Nahkampfangriff hier
        print("Attack");
    }

#endregion
}
