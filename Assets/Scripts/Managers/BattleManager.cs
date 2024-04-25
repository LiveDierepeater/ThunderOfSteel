using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<Weaponry, UnitHealth> OnAttackStarted;
    public event Action<Weaponry, UnitHealth> OnAttackStopped;
    public event Action<UnitHealth> OnUnitDied;
    
    private Dictionary<Weaponry, UnitHealth> activeWeapons = new Dictionary<Weaponry, UnitHealth>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartAttack(Weaponry attacker, UnitHealth target)
    {
        OnAttackStarted?.Invoke(attacker, target);
        RegisterAttack(attacker, target);
    }

    public void StopAttack(Weaponry attacker, UnitHealth target)
    {
        OnAttackStopped?.Invoke(attacker, target);
        UnregisterAttack(attacker);
    }

    public void NotifyDeath(UnitHealth target)
    {
        OnUnitDied?.Invoke(target);
        UnregisterByKill(target);
    }
    
    /// <summary>
    /// Registers an attacker 'Weaponry' and/or updates his target 'UnitHealth'
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    private void RegisterAttack(Weaponry attacker, UnitHealth target)
    {
        if ( ! activeWeapons.TryGetValue(attacker, out var targetsHealth))
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
        if (activeWeapons.TryGetValue(attacker, out var targetsHealth))
            activeWeapons.Remove(attacker);
    }

    /// <summary>
    /// Removes each attacker 'Weaponry' when a target 'UnitHealth' got killed
    /// </summary>
    /// <param name="killedTarget"></param>
    private void UnregisterByKill(UnitHealth killedTarget)
    {
        // This List will store the attackers 'Weaponry' who will be removed from activeWeapons 'Dictionary'
        var keysToRemove = new List<Weaponry>();
        
        // Returns if activeWeapons does not even have the killedTarget registered under an attacker
        // Adds every attacker/'Weaponry'/key-from-activeWeapons to keysToRemove<List>
        if (activeWeapons.ContainsValue(killedTarget))
        {
            foreach (var activeWeapon in activeWeapons.Keys)
                if (activeWeapons.TryGetValue(activeWeapon, out var target) == killedTarget)
                    keysToRemove.Add(activeWeapon);
        }
        else return;
        
        // Removes all killing attackers
        foreach (var weaponry in keysToRemove)
            activeWeapons.Remove(weaponry);
        
        keysToRemove.Clear();
    }
}
