using System.Collections.Generic;
using UnityEngine;

public class WeaponryHandler : MonoBehaviour
{
    private readonly List<Weaponry> _weapons = new();
    private List<Weaponry> inactiveWeapons = new();

    private void Start()
    {
        // Find all Weaponry objects as children of this object
        foreach (Transform child in transform)
        {
            var weaponry = child.GetComponent<Weaponry>();
            if (weaponry != null)
            {
                _weapons.Add(weaponry);
            }
        }
        
        TickManager.Instance.TickSystem.OnTick += UpdateTargets;
    }

    private void UpdateTargets()
    {
        // Find inactive Weapons which can search for a new target
        // Cashes 'inactiveWeapons.Count'
        inactiveWeapons = WeaponsSearchingForTarget();
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
            // Continues foreach, if nearbyUnit is in same Team
            //if (inactiveWeapons[0].IsTargetUnitInSameTeam(nearbyUnit.UnitPlayerID)) continue;
            
            // Goes through every weapon in 'inactiveWeapons'
            for (var index = 0; index < inactiveWeapons.Count; index++)
            {
                var inactiveWeapon = inactiveWeapons[index];
                
                // Continues for, if current 'inactiveWeapon' cannot damage 'nearbyUnit'
                if (!inactiveWeapon.CanWeaponryDamageTargetUnit(nearbyUnit)) continue;
                
                // Calculates squared distance to 'nearbyUnit'
                var distanceSqrt = Vector3.SqrMagnitude(nearbyUnit.transform.position - transform.position);
                
                // Skip itself, if distance to 'nearbyUnit' is too close
                if (distanceSqrt <= .2) continue;
                
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
            inactiveWeapons[index].AddWeaponryToBattleManager(closestEnemies[index]);
        }
    }

    private List<Weaponry> WeaponsSearchingForTarget()
    {
        var searchingWeapons = new List<Weaponry>();
        
        foreach (var weaponry in _weapons)
        {
            if (weaponry.GetTargetUnit() is not null) continue;
            searchingWeapons.Add(weaponry);
        }
        
        return searchingWeapons;
    }
}
