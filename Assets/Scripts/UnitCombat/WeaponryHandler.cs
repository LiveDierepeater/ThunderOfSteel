using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponryHandler : UnitSystem
{
    public Weaponry[] _weapons = new Weaponry[5];
    private Weaponry[] _freeWeapons = new Weaponry[5];
    private Weaponry[] _hullWeapons = new Weaponry[5];
    private Weaponry[] _turretWeapons = new Weaponry[5];
    private List<Weaponry> _inactiveWeapons = new();
    private readonly List<Weaponry> _activeWeapons = new();

    private UnitData.Type _unitType;

    private bool IsAnAPWeaponryActive;

    private void Start()
    {
        _unitType = Unit.UnitData.UnitType;
        
        
        

        Invoke(nameof(InitializeHandler), 0.05f);
        Invoke(nameof(InitializeOnCheckForEnemyUnit), 0.1f);
    }

    private void InitializeHandler()
    {
        InitializeWeaponryArray();
        Unit.Events.OnGetMaxAttackRange += GetMaxAttackRange;
        
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.Events.OnUnitDeath += HandleUnitDeath;
        Unit.Events.OnUnitFlee += HandleUnitFlee;
        Unit.Events.OnUnitOperational += HandleUnitOperational;
    }
    

    private void InitializeWeaponryArray()
    {
        int i = 0;

        int f = 0;
        int h = 0;
        int t = 0;
        
        // Find all Weaponry objects as children of this object
        foreach (Transform child in transform)
        {
            var weaponry = child.GetComponent<Weaponry>();
            if (weaponry != null)
            {
                switch (weaponry.WeaponryData.WeaponryBounds)
                {
                    case UnitWeaponry.Bounds.Free:
                        _freeWeapons[f] = weaponry;
                        f++;
                        break;
                    
                    case UnitWeaponry.Bounds.Hull:
                        _hullWeapons[h] = weaponry;
                        h++;
                        break;
                    
                    case UnitWeaponry.Bounds.Turret:
                        _turretWeapons[t] = weaponry;
                        t++;
                        break;
                }
                
                _weapons[i] = weaponry;
                i++;
            }
        }
        
        Array.Resize(ref _weapons, i);
        Array.Resize(ref _freeWeapons, f);
        Array.Resize(ref _hullWeapons, h);
        Array.Resize(ref _turretWeapons, t);
    }

    private void HandleUnitDeath()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.Events.OnUnitDeath -= HandleUnitDeath;
        Unit.Events.OnUnitFlee -= HandleUnitFlee;
        Unit.Events.OnUnitOperational -= HandleUnitOperational;
        Unit.Events.OnGetMaxAttackRange -= GetMaxAttackRange;
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
        foreach (var weaponry in _weapons) if (weaponry._targetUnit is not null) _activeWeapons.Add(weaponry);

        foreach (var activeWeapon in _activeWeapons)
        {
            if (activeWeapon.WeaponryData.ShellType == UnitWeaponry.Shells.APShell) IsAnAPWeaponryActive = true;
            else IsAnAPWeaponryActive = false;
        }
        
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        // Find inactive Weapons which can search for a new target
        // Cashes 'inactiveWeapons.Count'
        _inactiveWeapons = GetWeaponsSearchingForTarget();
        int inactiveWeaponsCount = _inactiveWeapons.Count;
        
        // Return, if there are no inactive weapons
        if (inactiveWeaponsCount == 0) return;
        
        // Cashes nearbyEnemies
        var nearbyUnits = SpatialHashManager.Instance.SpatialHash.GetNearbyUnitsFromDifferentTeams(transform.position, Unit.UnitPlayerID);
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
            if (Unit.Events.OnCheckForEnemyUnit?.Invoke(nearbyUnit.UnitPlayerID) == true) continue;
            
            // Continues for, if 'nearbyUnit' cannot get attacked because a building is blocking the vision
            //1<<LayerMask.NameToLayer("Buildings")
            if (Physics.Raycast(transform.position, nearbyUnit.transform.position - transform.position,
                    Vector3.Distance(transform.position, nearbyUnit.transform.position),
                    LayerMask.GetMask("Buildings")))
            {
                continue;
            }
            
            // Goes through every weapon in 'inactiveWeapons'
            for (var index = 0; index < _inactiveWeapons.Count; index++)
            {
                var inactiveWeapon = _inactiveWeapons[index];
                
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
            _inactiveWeapons[index].SetTarget(closestEnemies[index]);
        }
    }

    private void Update() => HandleWeaponryRotation();

    private void HandleWeaponryRotation()
    {
        // Rotate Turret to default rotation, when weapons are not fighting
        if (_inactiveWeapons.Count == _weapons.Length)
        {
            // Return, if Unit tries to attack an enemy unit
            if (Unit.CurrentUnitCommand == Unit.UnitCommands.Attack) return;
            
            // Return, if Turret is already looking straight
            if (Unit.Turret.rotation.eulerAngles == Vector3.zero) return;
            
            // Rotate Turret
            RotateTurretToDefault();
            
            // Return, if evey weapon is inactive
            return;
        }
        
        foreach (var weaponry in _activeWeapons)
        {
            // Continue for, if weaponry has no target
            if (weaponry._targetUnit is null) continue;
            
            // Continue for, if weaponry cannot damage 'weaponry._targetUnit'
            if ( ! weaponry.CanWeaponryDamageTargetUnit(weaponry._targetUnit)) continue;
            
            // Continue for, if weaponry is not an AP Type weaponry, so the AP Type weaponry has priority, EXCEPT the unit has the specific order to attack the target, which gets attacked by this non-AP-Shell-Type weaponry
            if (weaponry.WeaponryData.ShellType != UnitWeaponry.Shells.APShell && IsAnAPWeaponryActive)
                if (Unit.TargetUnit != weaponry._targetUnit)
                    continue;
            
            switch (weaponry.WeaponryData.WeaponryBounds)
            {
                // Continue for, if weaponry's bounds are free
                case UnitWeaponry.Bounds.Free:
                    continue;
                
                case UnitWeaponry.Bounds.Hull:
                {
                    // If Unit Type is Tank
                    if (_unitType == UnitData.Type.Tank)
                    {
                        // Return, if Unit is not standing
                        if (!Unit.TankMovement.IsUnitStanding()) continue;

                        RotateWeaponryBoundsTransform(weaponry, transform);
                    }
                    break;
                }
                case UnitWeaponry.Bounds.Turret:
                    // If Unit Type is Tank
                    if (_unitType == UnitData.Type.Tank)
                        RotateWeaponryBoundsTransform(weaponry, Unit.Turret);
                    break;
            }
        }
    }

    private void RotateWeaponryBoundsTransform(Weaponry weaponry, Transform tr)
    {
        float degreesPerSecond = Unit.UnitData.TurnSpeed * 0.1f * Time.deltaTime;
        Vector3 direction = weaponry._targetUnit.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRotation, degreesPerSecond);
    }

    private void RotateTurretToDefault()
    {
        float degreesPerSecond = Unit.UnitData.TurnSpeed * 0.1f * Time.deltaTime;
        Vector3 direction = transform.forward;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Unit.Turret.rotation = Quaternion.RotateTowards(Unit.Turret.rotation, targetRotation, degreesPerSecond);
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
            Unit.Events.OnCheckForEnemyUnit += CheckForPlayerUnit;
        
        else if (CompareTag("Untagged"))
            Unit.Events.OnCheckForEnemyUnit += CheckForAllyUnit;
        
        else if (CompareTag("AI"))
            Unit.Events.OnCheckForEnemyUnit += CheckForAIUnit;
    }

    private bool CheckForPlayerUnit(int unitPlayerID) => unitPlayerID == InputManager.Instance.Player.GetInstanceID();

    private bool CheckForAllyUnit(int unitPlayerID) => unitPlayerID == UnitManager.ALLY_ID;

    private bool CheckForAIUnit(int unitPlayerID) => unitPlayerID == UnitManager.AI_ID;
}
