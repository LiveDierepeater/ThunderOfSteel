using System.Collections;
using UnityEngine;

public class UHealth : UnitSystem
{
    // Stats Fields
    private int _maxHealth;
    private int _currentHealth;

    private const int _RegenerateHealthAmount = 3;

    private enum HealthState
    {
        Operational,
        Flee
    }
    private HealthState _unitHealthState;

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
        _unitHealthState = HealthState.Operational;

        Unit.Events.OnAttack += TakeDamage;
    }

    private void OnDisable() => Unit.Events.OnAttack -= TakeDamage;

#endregion

    private void TakeDamage(Vector3 projectilesOriginPosition , int amount)
    {
        _currentHealth -= amount;
        TickManager.Instance.TickSystem.OnTick -= RegenerateHealth;
        
        if (_currentHealth <= _maxHealth * 0.3f) CallUnitToFlee(projectilesOriginPosition);
        
        if (_currentHealth <= 0)
        {
            UnsubscribeUnit();
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(RegenerateHealthCooldown());
    }

    private void CallUnitToFlee(Vector3 projectilesOriginPosition)
    {
        // Return, if Unit is already fleeing
        if (_unitHealthState != HealthState.Operational) return;
        
        _unitHealthState = HealthState.Flee;
        Unit.Events.OnUnitFlee?.Invoke(projectilesOriginPosition);
    }

    private void UnsubscribeUnit()
    {
        // Stop Regeneration Coroutine
        StopAllCoroutines();
        TickManager.Instance.TickSystem.OnTick -= RegenerateHealth;
        
        // Notifying BattleManager about the Death of this.Unit
        BattleManager.Instance.NotifyDeath(Unit);
        
        // Removing this.Unit from SelectionManager
        SelectionManager.Instance.Deselect(Unit);
        SelectionManager.Instance.RemoveAvailableUnit(Unit);
        
        // Calls Event to USpottingSystem
        Unit.Events.OnHandleUnitDeathForSpotting?.Invoke();
        
        OnDisable();
    }

    private void RegenerateHealth()
    {
        // Add health-amount
        _currentHealth += _RegenerateHealthAmount;

        if (_currentHealth >= _maxHealth * 0.3f && _unitHealthState == HealthState.Flee)
        {
            _unitHealthState = HealthState.Operational;
            Unit.Events.OnUnitOperational?.Invoke();
        }
        
        // Return, if Health are not fully healed
        if (_currentHealth <= _maxHealth) return;
        
        // Health are fully regenerated
        _currentHealth = _maxHealth;
        TickManager.Instance.TickSystem.OnTick -= RegenerateHealth;
    }

    private IEnumerator RegenerateHealthCooldown()
    {
        yield return new WaitForSeconds(10f);
        TickManager.Instance.TickSystem.OnTick += RegenerateHealth;
    }
}
