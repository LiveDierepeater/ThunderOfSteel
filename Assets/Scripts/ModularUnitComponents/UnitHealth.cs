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
    }

    #endregion

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            DestroyUnit();
        }
    }

    private void DestroyUnit()
    {
        print(Unit.UnitData.UnitName + " is Destroyed");
        
        // TODO: Unsubscribe multiple Events.
        
        Destroy(gameObject);
    }
}
