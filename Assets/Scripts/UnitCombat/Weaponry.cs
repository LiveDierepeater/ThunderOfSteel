using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Weaponry : UnitSystem, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack
    
    [FormerlySerializedAs("_weaponryData")] public UnitWeaponry WeaponryData;
    private UnitWeaponry.Bounds _unitBounds;

    public float MaxAttackRange { get; private set; }

    public ArtilleryShell artilleryShellPrefab;
    public TankShell tankShellPrefab;
    
    public Unit _targetUnit { get; private set; }

    public delegate void OnTargetDeathDelegate();
    public OnTargetDeathDelegate OnLoosingTarget;
    
#region Internal Fields

    // Private Fields
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
    
    // Cashing Fields
    private int[] localArmorDamage;
    private Transform _oldMuzzleFlash;

#endregion

#region Initializing

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.Events.OnNewTargetUnit += SetTarget;
    }

    public void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.Events.OnNewTargetUnit -= SetTarget;
    }

    public void OnEnableWeaponry()
    {
        print("OnEnable");
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.Events.OnNewTargetUnit += SetTarget;
    }

    public void InitializeWeaponry(UnitWeaponry weaponryData)
    {
        WeaponryData = weaponryData;
        
        _damage_Infantry = WeaponryData.Damage_Infantry;
        _damage_Truck = WeaponryData.Damage_Truck;
        _damage_Building = WeaponryData.Damage_Building;
        _damage_Armor_Level_01 = WeaponryData.Damage_Armor_Level_01;
        _damage_Armor_Level_02 = WeaponryData.Damage_Armor_Level_02;
        _damage_Armor_Level_03 = WeaponryData.Damage_Armor_Level_03;
        _damage_Armor_Level_04 = WeaponryData.Damage_Armor_Level_04;
        _damage_Armor_Level_05 = WeaponryData.Damage_Armor_Level_05;
        _damage_Air = WeaponryData.Damage_Air;
        MaxAttackRange = WeaponryData.AttackRange;
        _coolDown = WeaponryData.CoolDown;
        
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
        
        localArmorDamage = _armorDamage;
        
        _unitBounds = WeaponryData.WeaponryBounds;
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
        if (_targetUnit is not null)
            MoveInRange();
        
        if (WeaponryData.ShellType == UnitWeaponry.Shells.APShell && Unit.gameObject.name == "M26 Pershing (1)")
        {
            print(_targetUnit);
            if (_targetUnit == null)
                CooldownManager.Instance._text.text = Unit + "'s target is: null";
            else
                CooldownManager.Instance._text.text = Unit + "'s target is: " + _targetUnit;
        }
    }

#endregion

#region External Called Logic

    public void SetTarget(Unit target)
    {
        // Informs BattleManager, if the target was not set to null
        if (target is not null) BattleManager.Instance.StartAttack(this, target);
        
        if (WeaponryData.ShellType == UnitWeaponry.Shells.APShell && Unit.gameObject.name == "M26 Pershing (1)")
            print("target set: " + _targetUnit);
        
        _targetUnit = target;
        OnLoosingTarget?.Invoke();
    }

    public void RemoveCooldownTime(float amount) => CurrentCoolDownTime -= amount;

#endregion

#region Intern Logic

    private void MoveInRange()
    {
        Unit.IsAttacking = false; // DEBUG
        
        if ( ! CanWeaponryAttackTarget(_targetUnit))
        {
            print("A");
            SetTarget(null);
            return;
        }
        
        var distanceToTarget = Vector3.Distance(transform.position, _targetUnit.transform.position);
        
        if (Unit.CurrentUnitCommand != Unit.UnitCommands.Attack)
        {
            if (WeaponryData.AttackRange < distanceToTarget)
            {
                print("B");
                SetTarget(null);
                return;
            }
        }
        
        if (CanAttack)
        {
            // Target is in 'AttackRange'
            if (distanceToTarget <= MaxAttackRange && _targetUnit.IsSpotted)
            {
                Attack(_targetUnit);
            }
            else if (distanceToTarget <= MaxAttackRange && ! _targetUnit.IsSpotted)
            {
                // Return, if weaponry's target is not the Unit's Attack-Target
                if (_targetUnit != Unit.TargetUnit)
                {
                    print("C");
                    SetTarget(null);
                    return;
                }
                
                Unit.Events.OnCommandToAttack?.Invoke(_targetUnit);
                Unit.IsAttacking = false; // DEBUG
            }
            else if (Unit.CurrentUnitCommand == Unit.UnitCommands.Attack)
            {
                // Move to target, till Unit is in 'AttackRange'
                Unit.Events.OnCommandToAttack?.Invoke(_targetUnit);
                Unit.IsAttacking = false; // DEBUG
            }
        }
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        if (IsWeaponsCoolDownActive()) return;
        
        // Return, if weaponry is not looking at target
        if ( ! IsWeaponryLookingAtTarget()) return;
        
        FireWeaponry(targetUnit);
        NewCoolDown();
        
        // DEBUG
        Unit.IsAttacking = true;
    }

    private void FireWeaponry(Unit target)
    {
        Projectile projectileInstance;

        switch (WeaponryData.ShellType)
        {
            case UnitWeaponry.Shells.Artillery:
            {
                projectileInstance = Instantiate(artilleryShellPrefab, Unit.ShellSpawnLocation.position, Quaternion.identity);
                
                if (projectileInstance is ArtilleryShell artilleryShell)
                    artilleryShell.MaxArcHeight = 15.0f; // Safely accessing the specific property
                
                InitializeProjectile(projectileInstance, target);
                projectileInstance.InitializeWeaponryEvents(this);
                projectileInstance.InitializeArmorDamage(_armorDamage);
                
                break;
            }
            case UnitWeaponry.Shells.APShell or UnitWeaponry.Shells.HEShell:
            {
                // Returns, if '_targetUnit' is not spotted anymore and sets '_targetUnit' to null
                // if ( ! _targetUnit.IsSpotted)
                // {
                //     print("D");
                //     SetTarget(null);
                //     return;
                // }
                projectileInstance = Instantiate(tankShellPrefab, Unit.ShellSpawnLocation.position, Quaternion.identity);
                
                InitializeProjectile(projectileInstance, target);
                projectileInstance.InitializeWeaponryEvents(this);
                projectileInstance.InitializeArmorDamage(_armorDamage);
                
                break;
            }
        }

        Invoke(nameof(DestroyMuzzleFlash), 0.5f);
        
        _oldMuzzleFlash = Instantiate(WeaponryData.MuzzleFlash_Prefab, Unit.ShellSpawnLocation.position, Unit.ShellSpawnLocation.rotation).transform;
    }

    private void InitializeProjectile(Projectile projectileInstance, Unit target)
    {
        projectileInstance.InitialSpeed = WeaponryData.ProjectileSpeed;
        projectileInstance.Target = target;
    }

    private void DestroyMuzzleFlash() => Destroy(_oldMuzzleFlash.gameObject);

    #region Cooldown Management

    private void NewCoolDown()
    {
        ResetCoolDownTime();
        StartCoolDown();
    }

    private void ResetCoolDownTime() => CurrentCoolDownTime = _coolDown;

    private void StartCoolDown() => CooldownManager.Instance.StartCooldown(GetInstanceID(), this);

#endregion

#endregion

#region Extracted Return Methods

    /// <summary>
    /// <para>Returns false, when 'targetUnit' is an ally</para>
    /// <para>Returns false, when this weaponry cannot attack the armor of the 'targetUnit'</para>
    /// </summary>
    /// <param name="targetUnit"></param>
    /// <returns></returns>
    private bool CanWeaponryAttackTarget(Unit targetUnit) => targetUnit.UnitPlayerID != Unit.UnitPlayerID && localArmorDamage[(int)targetUnit.UnitData.Armor] >= 0;

    private bool IsWeaponsCoolDownActive() => CooldownManager.Instance.IsCooldownActive(GetInstanceID());

    public bool CanWeaponryDamageTargetUnit(Unit targetUnit) => localArmorDamage[(int)targetUnit.UnitData.Armor] >= 0;

    private bool IsWeaponryLookingAtTarget()
    {
        Transform tr = Unit.transform;
        
        if (_unitBounds == UnitWeaponry.Bounds.Turret) tr = Unit.Turret;
        
        var directionToTarget = (_targetUnit.transform.position - tr.position).normalized;
        
        if (Vector3.Dot(tr.forward, directionToTarget) >= 0.985f)
            return true;
        
        return false;
    }

#endregion
}
