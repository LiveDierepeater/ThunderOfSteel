using System;
using System.Linq;
using UnityEngine;

public class USpottingSystem : UnitSystem
{
    private float SpottingRange { get; set; }
    private LayerMask _unitsLayer;
    private LayerMask _obstacleLayer;

    public Action OnSpotterUnitDeath;
    
    private void Start()
    {
        TickManager.Instance.TickSystem.OnTickBegin += HandleTick;
        
        InitializeSpottingRange();
        
        _obstacleLayer = InputManager.Instance.Player.RaycastLayerMask;
        _unitsLayer = InputManager.Instance.Player.unitsLayerMask;
        Unit.Events.OnHandleUnitDeathForSpotting += HandleUnitDeath;
    }

    private void InitializeSpottingRange()
    {
        SpottingRange = Unit.Events.OnGetMaxAttackRange.Invoke() * 0.75f;
        
        if (SpottingRange < 150f)
            SpottingRange = 150f;
    }

    private void HandleUnitDeath()
    {
        OnSpotterUnitDeath?.Invoke();
        Unit.Events.OnUnitDeath?.Invoke();
        TickManager.Instance.TickSystem.OnTickBegin -= HandleTick;
        Unit.Events.OnHandleUnitDeathForSpotting -= HandleUnitDeath;
    }

    private void HandleTick()
    {
        HideIfUnitIsNotSpottedAnymore();
        SpotEnemyUnitsInRange();
    }

    /// <summary>
    /// This Method sets Unit.IsSpotted = false, if Unit.SpottingUnit cannot spot Unit anymore
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void HideIfUnitIsNotSpottedAnymore()
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
        var colliderArray = Physics.OverlapSphere(transform.position, SpottingRange, _unitsLayer);
        
        foreach (var collider1 in colliderArray)
        { 
            if (collider1.TryGetComponent(out Unit targetUnit))
            {
                // Return, if 'targetUnit' is this Unit
                if (targetUnit == Unit) continue;
                
                // Return, if 'targetUnit' is in same team
                if (targetUnit.UnitPlayerID == Unit.UnitPlayerID) continue;
                
                // Return, if this unit is a unit from player and the 'targetUnit' is a unit from an ally
                if (Unit.UnitPlayerID == InputManager.Instance.Player.GetInstanceID() && targetUnit.UnitPlayerID == UnitManager.ALLY_ID) continue;
                
                // Return, if this unit is an ally and the 'targetUnit' is a unit from player
                if (Unit.UnitPlayerID == UnitManager.ALLY_ID && targetUnit.UnitPlayerID == InputManager.Instance.Player.GetInstanceID()) continue;

                // Return, if 'targetUnit' is already spotted
                if (targetUnit.IsSpotted) continue;
                
                // Spots 'targetUnit'
                if (TrySpotUnit(Unit, targetUnit))
                {
                    targetUnit.IsSpotted = true;
                    targetUnit.SpottingUnit = Unit;
                    OnSpotterUnitDeath += targetUnit.USpottingSystem.DeleteSpotterUnitReference;
                }
            }
        }
    }

    private bool TrySpotUnit(Unit spottingUnit, Unit targetUnit)
    {
        float forestThickness = CalculateForestThickness(spottingUnit.transform.position, targetUnit.transform.position);
        
        // Return, if 'targetUnit' cannot get spotted. Likely because of a building is blocking the sight
        if (forestThickness < 0)
            return false;
        
        if (spottingUnit.USpottingSystem.SpottingRange - forestThickness >
            Vector3.Distance(spottingUnit.transform.position, targetUnit.transform.position))
            return true;
        return false;
    }

    private float CalculateForestThickness(Vector3 unitAPosition, Vector3 unitBPosition)
    {
        // Forward raycast from unit A to unit B
        RaycastHit[] forwardHits = Physics.RaycastAll(unitAPosition, unitBPosition - unitAPosition, Vector3.Distance(unitAPosition, unitBPosition), _obstacleLayer);
        
        // Return -1f, if there is a building in sight
        if (forwardHits.Any(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("Buildings")))
            // Building blocks line of sight
            return -1f; // Negative value as an indicator for blocked sight
        
        LayerMask forestLayer = _obstacleLayer & ~(1 << LayerMask.NameToLayer("Buildings"));
        
        // Backward raycast from unit B to unit A
        RaycastHit[] backwardHits = Physics.RaycastAll(unitBPosition, unitAPosition - unitBPosition, Vector3.Distance(unitAPosition, unitBPosition), forestLayer);
        
        // Sort the hits by distance to the respective starting point
        forwardHits = forwardHits.OrderBy(hit => Vector3.Distance(hit.point, unitAPosition)).ToArray();
        backwardHits = backwardHits.OrderBy(hit => Vector3.Distance(hit.point, unitAPosition)).ToArray();
        
        // Check if an Unit sits in forest
        bool unitAInForest = Physics.CheckSphere(unitAPosition, 0.5f, forestLayer);
        bool unitBInForest = Physics.CheckSphere(unitBPosition, 0.5f, forestLayer);
        
        // Sampling the positions of units if they are in a forest
        if (unitAInForest)
        {
            // Unit A is in the forest, add its position to the top of the forwardHits list
            forwardHits = forwardHits.Prepend(new RaycastHit { point = unitAPosition }).ToArray();
        }
        if (unitBInForest)
        {
            // Unit B is in the forest, add its position to the top of the backwardHits list
            backwardHits = backwardHits.Prepend(new RaycastHit { point = unitBPosition }).ToArray();
        }
        
        // Calculating the thickness of forests along the line of sight
        float totalForestThickness = CalculateThicknessFromHits(forwardHits, backwardHits);
        
        return totalForestThickness;
    }

    private float CalculateThicknessFromHits(RaycastHit[] forwardHits, RaycastHit[] backwardHits)
    {
        float totalForestThickness = 0f;
        int hitCount = Mathf.Min(forwardHits.Length, backwardHits.Length);
        
        // DEBUG: The RaycastHit-Arrays do not share the same length, although they should!
        // if (forwardHits.Length != backwardHits.Length)
        //     print("Something went wrong! forwardHits: " + forwardHits.Length + " | backwardHits: " + backwardHits.Length);
        
        for (int i = 0; i < hitCount; i++)
        {
            float distance = Vector3.Distance(forwardHits[i].point, backwardHits[i].point);
            totalForestThickness += distance;
        }
        
        return totalForestThickness;
    }

    private void DeleteSpotterUnitReference()
    {
        Unit.IsSpotted = false;
        Unit.SpottingUnit = null;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SpottingRange);
    }
}
