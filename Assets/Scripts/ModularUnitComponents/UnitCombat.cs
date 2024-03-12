using System;
using UnityEngine;

public class UnitCombat : UnitSystem, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack

    public float MaxAttackRange { get; private set; }

#region Internal Fields

    // Private Fields
    private Unit _targetUnit;
    private UnitWeaponry _weaponryData;

    private int _damage_Infantry;
    private int _damage_Truck;
    private int _damage_Building;
    private int _damage_Armor_Level_01;
    private int _damage_Armor_Level_02;
    private int _damage_Armor_Level_03;
    private int _damage_Armor_Level_04;
    private int _damage_Armor_Level_05;
    private int _damage_Air;

    private readonly int[] _armorDamage = new int[9];

    private float _coolDown;
    public float CurrentCoolDownTime { get; private set; }

#endregion

#region Initializing

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit += SetTarget;
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit -= SetTarget;
    }

    private void InitializeWeaponry()
    {
        _damage_Infantry = _weaponryData.Damage_Infantry;
        _damage_Truck = _weaponryData.Damage_Truck;
        _damage_Building = _weaponryData.Damage_Building;
        _damage_Armor_Level_01 = _weaponryData.Damage_Armor_Level_01;
        _damage_Armor_Level_02 = _weaponryData.Damage_Armor_Level_02;
        _damage_Armor_Level_03 = _weaponryData.Damage_Armor_Level_03;
        _damage_Armor_Level_04 = _weaponryData.Damage_Armor_Level_04;
        _damage_Armor_Level_05 = _weaponryData.Damage_Armor_Level_05;
        _damage_Air = _weaponryData.Damage_Air;
        MaxAttackRange = _weaponryData.AttackRange;
        _coolDown = _weaponryData.CoolDown;

        // Set '_armorDamage'
        _armorDamage[0] = _damage_Infantry;
        _armorDamage[1] = _damage_Truck;
        _armorDamage[2] = _damage_Building;
        _armorDamage[3] = _damage_Armor_Level_01;
        _armorDamage[4] = _damage_Armor_Level_02;
        _armorDamage[5] = _damage_Armor_Level_03;
        _armorDamage[6] = _damage_Armor_Level_04;
        _armorDamage[7] = _damage_Armor_Level_05;
        _armorDamage[8] = _damage_Air;
    }

    private void OnValidate()
    {
        // Checks if there are enough armor types
        var armorTypeMemberCount = Enum.GetNames(typeof(UnitData.Armors)).Length;

        if (_armorDamage.Length != armorTypeMemberCount)
            throw new NotImplementedException(
                "The '_armorDamage.Length' doesn't match the 'UnitData.Armors.Length' anymore!");
    }

#endregion

#region UPDATES

    private void HandleTick()
    {
        switch (_targetUnit)
        {
            case not null: // Unit has target
                // TODO: Here has to be a switch-case for the UnitData.UnitCommands
                MoveInRange();
                break;

            case null: // Unit has NO target
                CheckForNewTargetInRange();
                break;
        }
    }

#endregion

#region External Called Logic

    public void SetTarget(Unit target)
    {
        _targetUnit = target;
    }

    public void SetWeaponryData(UnitWeaponry weaponryData)
    {
        _weaponryData = weaponryData;
        InitializeWeaponry();
    }

    public void RemoveCooldownTime(float amount)
    {
        CurrentCoolDownTime -= amount;
    }

#endregion

#region Intern Logic

    private void MoveInRange()
    {
        Unit.IsAttacking = false; // DEBUG

        if ( ! CanWeaponryAttackTarget(_targetUnit))
        {
            SetTarget(null);
            return;
        }

        if (Unit.UnitData.CurrentUnitCommand != UnitData.UnitCommands.Attack)
        {
            if (_weaponryData.AttackRange < Vector3.Distance(transform.position, _targetUnit.transform.position))
            {
                SetTarget(null);
                return;
            }
        }

        if (_targetUnit is not null && CanAttack)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);

            // Target is in 'AttackRange'
            if (distanceToTarget <= MaxAttackRange)
            {
                Attack(_targetUnit);
            }
            else if (Unit.UnitData.CurrentUnitCommand == UnitData.UnitCommands.Attack)
            {
                // Move to target, till Unit is in 'AttackRange'

                Unit.UnitData.Events.OnAttackUnit?.Invoke(_targetUnit.transform.position);
                Unit.IsAttacking = false; // DEBUG
            }
        }
    }

    private void CheckForNewTargetInRange()
    {
        if (_targetUnit is not null) return;

        var nearbyObjects =
            SpatialHashManager.Instance.SpatialHash.GetNearbyUnitObjectsInNearbyHashKeys(transform.position);
        GameObject closestEnemy = null;
        var closestDistance = 1000f;

        foreach (var nearbyObject in nearbyObjects)
        {
            var distance = Vector3.Distance(transform.position, nearbyObject.transform.position);

            if (nearbyObject == transform.root.gameObject)
                continue; // Continue, when this 'nearbyObject' is 'this.gameObject'
            
            if ( ! CanWeaponryAttackTarget(nearbyObject.GetComponent<Unit>()))
                continue; // Continue, when this 'nearbyObject' cannot be attacked by weaponry

            if ( ! (distance < closestDistance))
                continue; // Continue, when this 'nearbyObject' is not closer than the closest 'nearbyObject'
            
            if (MaxAttackRange < distance)
                continue; // Continue, when this 'nearbyObject' is not close enough to get attacked

            closestDistance = distance;
            closestEnemy = nearbyObject;
        }

        if (closestEnemy is null) return;

        _targetUnit = closestEnemy.GetComponent<Unit>();
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        if (IsWeaponsCoolDownActive()) return;
        
        ApplyDamageToTarget();
        NewCoolDown();
        
        //Unit.UnitData.Events.OnStopUnit?.Invoke();
        // DEBUG
        Unit.IsAttacking = true;
    }

    private void ApplyDamageToTarget()
    {
        _targetUnit.UnitData.Events.OnAttack?.Invoke(_armorDamage[(int)_targetUnit.UnitData.Armor]);
    }

#region Cooldown Management

    private void NewCoolDown()
    {
        ResetCoolDownTime();
        StartCoolDown();
    }

    private void ResetCoolDownTime()
    {
        CurrentCoolDownTime = _coolDown;
    }

    private void StartCoolDown()
    {
        CooldownManager.Instance.StartCooldown(GetInstanceID(), this);
    }

#endregion

#endregion

#region Extracted Return Methods

    private float GetMaxAttackRange() // Returns the maximal 'attackRange' out of the multiple weapons an Unit can have
    {
        float currentAttackRange = 0;

        foreach (var weaponry in Unit.UnitData.UnitWeaponry)
        {
            if (weaponry.AttackRange > currentAttackRange)
                currentAttackRange = weaponry.AttackRange;
        }

        return currentAttackRange;
    }

    /// <summary>
    /// <para>Returns false, when this weaponry cannot attack the armor of the 'targetUnit'</para>
    /// <para>Returns false, when 'targetUnit' is an ally</para>
    /// </summary>
    /// <param name="targetUnit"></param>
    /// <returns></returns>
    private bool CanWeaponryAttackTarget(Unit targetUnit)
    {
        if (targetUnit.UnitData.PlayerID != Unit.UnitData.PlayerID)
            return _armorDamage[(int)targetUnit.UnitData.Armor] >= 0;
        return false;
    }

    private bool IsWeaponsCoolDownActive()
    {
        return CooldownManager.Instance.IsCooldownActive(GetInstanceID());
    }

#endregion
}
