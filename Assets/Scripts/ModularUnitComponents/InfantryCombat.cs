using UnityEngine;

public class InfantryCombat : MonoBehaviour, IAttackBehavior
{
    public bool CanAttack => true; // Logic for beeing ready to Attack

    public float AttackRange { get; private set; }
    
    // Private Fields
    private Unit _unit;
    private Unit _targetUnit;
    
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
    
    private void HandleTick()
    {
        MoveInRange();
    }
    
    private void MoveInRange()
    {
        print(_targetUnit);
        if (_targetUnit is not null && CanAttack)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
            if (distanceToTarget <= AttackRange)
            {
                // Ziel ist in Angriffsreichweite
                Attack(_targetUnit);
            }
            else
            {
                // Bewege dich zum Ziel, bis du in Angriffsreichweite bist
                _unit.CommandToDestination(_targetUnit.transform.position);
            }
        }
    }
    
    public void SetTarget(Unit target)
    {
        _targetUnit = target;
    }
    
    public void Attack(Unit target)
    {
        // Implementiere den Nahkampfangriff hier
        print("Attack");
    }
}
