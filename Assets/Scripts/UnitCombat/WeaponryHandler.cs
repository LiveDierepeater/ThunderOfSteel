using System.Collections.Generic;
using UnityEngine;

public class WeaponryHandler : UnitSystem
{
    private Weaponry[] _weapons = new Weaponry[5];
    private List<Weaponry> inactiveWeapons = new();

    private void Start()
    {
        InitializeWeaponryArray();
        
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnUnitDeath += HandleUnitDeath;
        Unit.UnitData.Events.OnUnitFlee += HandleUnitFlee;
        Unit.UnitData.Events.OnUnitOperational += HandleUnitOperational;
        Unit.UnitData.Events.OnGetMaxAttackRange += GetMaxAttackRange;

        Invoke(nameof(InitializeOnCheckForEnemyUnit), 0.1f);
    }

    private void InitializeWeaponryArray()
    {
        int i = 0;
        // Find all Weaponry objects as children of this object
        foreach (Transform child in transform)
        {
            var weaponry = child.GetComponent<Weaponry>();
            if (weaponry != null)
            {
                _weapons[i] = weaponry;
                i++;
            }
        }
        System.Array.Resize(ref _weapons, i);
    }

    private void HandleUnitDeath()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnUnitDeath -= HandleUnitDeath;
        Unit.UnitData.Events.OnUnitFlee -= HandleUnitFlee;
        Unit.UnitData.Events.OnUnitOperational -= HandleUnitOperational;
        Unit.UnitData.Events.OnGetMaxAttackRange -= GetMaxAttackRange;
    }

    private void HandleUnitFlee(Vector3 projectileOrigin)
    {
        foreach (var weaponry in _weapons)
        {
            weaponry.SetTarget(null);
            weaponry.OnDisable();
            weaponry.enabled = false;
        }
    }

    private void HandleUnitOperational()
    {
        foreach (var weaponry in _weapons)
        {
            weaponry.OnEnableWeaponry();
            weaponry.enabled = true;
        }
    }

    private void HandleTick()
    {
        // DEBUG -> Updating Weaponry's 'localPlayerID'
        foreach (var weaponry in _weapons) weaponry.localPlayerID = Unit.UnitPlayerID;
        
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        // Find inactive Weapons which can search for a new target
        // Cashes 'inactiveWeapons.Count'
        inactiveWeapons = GetWeaponsSearchingForTarget();
        int inactiveWeaponsCount = inactiveWeapons.Count;
        
        // Return, if there are no inactive weapons
        if (inactiveWeaponsCount == 0) return;
        
        // Cashes nearbyEnemies
        var nearbyUnits = SpatialHashManager.Instance.SpatialHash.GetNearbyUnitsFromDifferentTeams(transform.position, inactiveWeapons[0].localPlayerID);
        var closestEnemies = new Unit[inactiveWeaponsCount];
        var closestDistanceSqrt = new float[inactiveWeaponsCount];
        
        // Sets all closestDistanceSqrt to 1000000f
        for (int index = 0; index < inactiveWeaponsCount; index++) closestDistanceSqrt[index] = 1000000f;
        
        // Iterates through every Unit in nearby HashKeys
        foreach (var nearbyUnit in nearbyUnits)
        {
            // Continues for, if current 'nearbyUnt' is not spotted
            if ( ! nearbyUnit.IsSpotted) continue;
            
            // Continues for, if 'nearbyUnit' cannot get attacked by his team member
            if (Unit.UnitData.Events.OnCheckForEnemyUnit?.Invoke(nearbyUnit.UnitPlayerID) == true) continue;
            
            //1<<LayerMask.NameToLayer("Buildings")
            if (Physics.Raycast(transform.position, nearbyUnit.transform.position - transform.position,
                    Vector3.Distance(transform.position, nearbyUnit.transform.position),
                    LayerMask.GetMask("Buildings")))
            {
                continue;
            }
            
            // Goes through every weapon in 'inactiveWeapons'
            for (var index = 0; index < inactiveWeapons.Count; index++)
            {
                var inactiveWeapon = inactiveWeapons[index];
                
                // Continues for, if current 'inactiveWeapon' cannot damage 'nearbyUnit'
                if ( ! inactiveWeapon.CanWeaponryDamageTargetUnit(nearbyUnit)) continue;
                
                // Calculates squared distance to 'nearbyUnit'
                var distanceSqrt = Vector3.SqrMagnitude(nearbyUnit.transform.position - transform.position);
                
                // Stores the enemy 'nearbyUnit' and it's distance to it in the current index slot
                if (distanceSqrt < closestDistanceSqrt[index] && distanceSqrt <= inactiveWeapon.MaxAttackRange * inactiveWeapon.MaxAttackRange)
                {
                    closestDistanceSqrt[index] = distanceSqrt;
                    closestEnemies[index] = nearbyUnit;
                }
            }
        }
        
        // Goes through each 'inactiveWeapon'
        for (int index = 0; index < inactiveWeaponsCount; index++)
        {
            // Continues for, if Search-Iteration has not found an enemy targetUnit for this current Weaponry
            if (closestEnemies[index] is null) continue;
            
            // Sets the enemy targetUnit for the current Weaponry
            // Adds the current Weaponry to the BattleManager.cs
            inactiveWeapons[index].SetTarget(closestEnemies[index]);
        }
    }

    private List<Weaponry> GetWeaponsSearchingForTarget()
    {
        var searchingWeapons = new List<Weaponry>();
        
        foreach (var weaponry in _weapons)
        {
            if (weaponry._targetUnit is not null) continue;
            searchingWeapons.Add(weaponry);
        }
        
        return searchingWeapons;
    }

    private float GetMaxAttackRange()
    {
        var maxAttackRange = - 1f;
        
        foreach (var weaponry in _weapons)
            if (weaponry.MaxAttackRange > maxAttackRange)
                maxAttackRange = weaponry.MaxAttackRange;
        
        return maxAttackRange;
    }

    private void InitializeOnCheckForEnemyUnit()
    {
        if (CompareTag("Ally"))
            Unit.UnitData.Events.OnCheckForEnemyUnit += CheckForPlayerUnit;
        
        else if (CompareTag("Untagged"))
            Unit.UnitData.Events.OnCheckForEnemyUnit += CheckForAllyUnit;
        
        else
            Unit.UnitData.Events.OnCheckForEnemyUnit += null;
    }
    
    private bool CheckForPlayerUnit(int unitPlayerID) => unitPlayerID == InputManager.Instance.Player.GetInstanceID();
    
    private bool CheckForAllyUnit(int unitPlayerID) => unitPlayerID == UnitManager.ALLY_ID;
}
