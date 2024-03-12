public class UnitHealth : UnitSystem
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
        
        if (_currentHealth <= 0)
        {
            DestroyUnit();
        }
        
        print(Unit.UnitData.UnitName + " took " + amount + " damage!");
    }

    private void DestroyUnit()
    {
        print(Unit.UnitData.UnitName + " is Destroyed");
        
        // TODO: Unit and it's components have to unsubscribe from multiple Events here.
        Unit.UnitData.Events.OnAttack -= TakeDamage;
        
        Destroy(gameObject);
    }
}
