using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<Weaponry, Unit> OnAttackStarted;
    public event Action<Weaponry, Unit> OnAttackStopped;
    public event Action<Unit> OnUnitDied;
    
    private readonly Dictionary<Weaponry, Unit> activeWeapons = new Dictionary<Weaponry, Unit>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartAttack(Weaponry attacker, Unit target)
    {
        OnAttackStarted?.Invoke(attacker, target);
        RegisterAttack(attacker, target);
    }

    public void StopAttack(Weaponry attacker, Unit target)
    {
        OnAttackStopped?.Invoke(attacker, target);
        UnregisterAttack(attacker);
    }

    public void NotifyDeath(Unit target)
    {
        OnUnitDied?.Invoke(target);
        UnregisterByKill(target);
    }
    
    /// <summary>
    /// Registers an attacker 'Weaponry' and/or updates his target 'UnitHealth'
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    private void RegisterAttack(Weaponry attacker, Unit target)
    {
        if ( ! activeWeapons.TryGetValue(attacker, out _))
            activeWeapons.Add(attacker, target);
        else
            activeWeapons[attacker] = target;
    }

    /// <summary>
    /// Removes or Unregisters an attacker 'Weaponry'
    /// </summary>
    /// <param name="attacker"></param>
    private void UnregisterAttack(Weaponry attacker)
    {
        if (activeWeapons.TryGetValue(attacker, out _))
            activeWeapons.Remove(attacker);
    }

    /// <summary>
    /// Removes each attacker 'Weaponry' when a target 'UnitHealth' got killed
    /// </summary>
    /// <param name="killedTarget"></param>
    private void UnregisterByKill(Unit killedTarget)
    {
        // This List will store the attackers 'Weaponry' who will be removed from activeWeapons 'Dictionary'
        var keysToRemove = new List<Weaponry>();
        
        // Returns if activeWeapons does not even have the killedTarget registered under an attacker
        // Adds every attacker/'Weaponry'/key-from-activeWeapons to keysToRemove<List>
        if (activeWeapons.ContainsValue(killedTarget))
        {
            foreach (var activeWeapon in activeWeapons.Keys)
                if (activeWeapons.TryGetValue(activeWeapon, out _) == killedTarget)
                    keysToRemove.Add(activeWeapon);
        }
        else return;
        
        // Removes all killing attackers
        // Sets the _targetUnit in Weaponry.cs to null
        foreach (var weaponry in keysToRemove)
        {
            activeWeapons.Remove(weaponry);
            weaponry.SetTarget(null);
        }
        
        keysToRemove.Clear();
    }
}
