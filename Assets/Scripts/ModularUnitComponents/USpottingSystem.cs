using System;
using UnityEngine;

public class USpottingSystem : UnitSystem
{
    private float _spottingRange;

    [SerializeField] private bool DrawGizmos = true;

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTickBegin += HandleTick;
        _spottingRange = Unit.UnitData.SpottingRange;
    }

    private void HandleTick()
    {
        HideIfNotSpotted();
        SpotEnemyUnitsInRange();
    }

    /// <summary>
    /// This Method sets Unit.IsSpotted = false, if Unit.SpottingUnit cannot spot Unit anymore
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void HideIfNotSpotted()
    {
        // Return, if this Unit is not spotted yet
        if ( ! Unit.IsSpotted) return;
        
        // Debug
        if (Unit.SpottingUnit is null) throw new Exception("Attention! Here could have happened a bug! Unit is spotted but has no 'SpottingUnit'.");
        
        // Return, if 'SpottingUnit' still spots this Unit
        if (TrySpotUnit(Unit.SpottingUnit, Unit)) return;
        
        Unit.IsSpotted = false;
        Unit.SpottingUnit = null;
    }

    private void SpotEnemyUnitsInRange()
    {
        var colliderArray = Physics.OverlapSphere(transform.position, _spottingRange, InputManager.Instance.Player.unitsLayerMask);
        
        foreach (var collider1 in colliderArray)
        { 
            if (collider1.TryGetComponent(out Unit targetUnit))
            {
                // Return, if 'targetUnit' is this Unit
                if (targetUnit.UnitData.InstanceID == Unit.UnitData.InstanceID) continue;
                
                // Return, if 'targetUnit' is in same team
                if (targetUnit.UnitPlayerID == Unit.UnitPlayerID) continue;
                
                // Return, if 'targetUnit' is already spotted
                if (targetUnit.IsSpotted) continue;
                
                // Spots 'targetUnit'
                if (TrySpotUnit(Unit, targetUnit))
                {
                    targetUnit.IsSpotted = true;
                    targetUnit.SpottingUnit = Unit;
                }
            }
        }
    }

    private bool TrySpotUnit(Unit spottingUnit, Unit targetUnit)
    {
        var directionToTargetUnit = (targetUnit.transform.position - spottingUnit.transform.position).normalized;
        if (Physics.Raycast(spottingUnit.transform.position, directionToTargetUnit, _spottingRange, InputManager.Instance.Player.RaycastLayerMask))
            
        
        if (Vector2.Distance
            (
                new Vector2(spottingUnit.transform.position.x, spottingUnit.transform.position.z), 
                new Vector2(targetUnit.transform.position.x, targetUnit.transform.position.z)) 
                < spottingUnit.UnitData.SpottingRange
            )
            return true;
        return false;
    }

    private void OnDrawGizmos()
    {
        if ( ! DrawGizmos) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _spottingRange);
    }
}
