public class UHealth : UnitSystem
{
    // Stats Fields
    private int _maxHealth;
    private int _currentHealth;

    #region Initializing

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    
    private void Initialize()
    {
        _maxHealth = Unit.UnitData.MaxHealth;
        _currentHealth = Unit.UnitData.MaxHealth;

        Unit.UnitData.Events.OnAttack += TakeDamage;
    }

    private void OnDisable()
    {
        Unit.UnitData.Events.OnAttack -= TakeDamage;
    }

#endregion

    private void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        print(Unit.UnitData.UnitName + " took " + amount + " damage!");
        
        if (_currentHealth <= 0)
        {
            UnsubscribeUnit();
        }
    }

    private void UnsubscribeUnit()
    {
        // Notifying BattleManager about the Death of this.Unit
        BattleManager.Instance.NotifyDeath(Unit);
        
        // Removing this.Unit from SelectionManager
        SelectionManager.Instance.Deselect(Unit);
        SelectionManager.Instance.RemoveAvailableUnit(Unit);
        
        // Calls Event to Unit-Instance
        Unit.UnitData.Events.OnUnitDeath?.Invoke();
        
        print(Unit.UnitData.UnitName + " is Destroyed");
        
        // TODO: Unit and it's components have to unsubscribe from multiple Events here.
        Unit.UnitData.Events.OnAttack -= TakeDamage;
    }
}
