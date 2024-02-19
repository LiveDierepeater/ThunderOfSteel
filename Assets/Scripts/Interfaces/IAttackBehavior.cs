public interface IAttackBehavior
{
    void Initialize(UnitData data);
    void Attack(Unit target);
    void SetTarget(Unit target);
    bool CanAttack { get; }
    float AttackRange { get; }
}
