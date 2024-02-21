public interface IAttackBehavior
{
    void Attack(Unit target);
    void SetTarget(Unit target);
    bool CanAttack { get; }
    float AttackRange { get; }
}
