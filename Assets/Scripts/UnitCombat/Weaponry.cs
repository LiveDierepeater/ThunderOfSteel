using System;
using UnityEngine;

public class Weaponry : UnitSystem, IAttackBehavior
{
    public bool CanAttack => true; // Logic for being ready to Attack

    public float MaxAttackRange { get; private set; }

    public ArtilleryShell artilleryShellPrefab;
    public TankShell tankShellPrefab;
    
    public Unit _targetUnit { get; private set; }

    public delegate void OnTargetDeathDelegate();
    public OnTargetDeathDelegate OnLoosingTarget;
    
#region Internal Fields

    // Private Fields
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
    
    // Cashing Fields
    public int localPlayerID;
    private int[] localArmorDamage;

#endregion

#region Initializing

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit += SetTarget;

        localPlayerID = Unit.UnitData.PlayerID;
        localArmorDamage = _armorDamage;
    }

    public void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit -= SetTarget;
    }

    public void OnEnableWeaponry()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnNewTargetUnit += SetTarget;
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
        if (_targetUnit is not null)
            if (_targetUnit.transform.gameObject.activeSelf) // Unit has target
                MoveInRange();
            else                                             // Unit is inactive (dead)
                SetTarget(null);
    }

#endregion

#region External Called Logic

    public void SetTarget(Unit target)
    {
        // Informs BattleManager, if the target was not set to null
        if (target is not null) BattleManager.Instance.StartAttack(this, target);
        
        _targetUnit = target;
        OnLoosingTarget?.Invoke();
    }

    public void SetWeaponryData(UnitWeaponry weaponryData)
    {
        _weaponryData = weaponryData;
        InitializeWeaponry();
    }

    public void RemoveCooldownTime(float amount) => CurrentCoolDownTime -= amount;

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

        if (CanAttack)
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
                print("invoked");
                Unit.UnitData.Events.OnCommandToDestination?.Invoke(_targetUnit.transform.position);
                Unit.IsAttacking = false; // DEBUG
            }
            
            // Returns, if '_targetUnit' is not spotted and the ShellType of this weaponry is not an artillery shell
            //if ( ! _targetUnit.IsSpotted && _weaponryData.ShellType != UnitWeaponry.Shells.Artillery) SetTarget(null);
        }
    }

#endregion

#region Extracted Logic Methods

    public void Attack(Unit targetUnit)
    {
        if (IsWeaponsCoolDownActive()) return;

        FireWeaponry(targetUnit);
        NewCoolDown();
        
        // DEBUG
        Unit.IsAttacking = true;
    }

    private void FireWeaponry(Unit target)
    {
        Projectile projectileInstance;

        switch (_weaponryData.ShellType)
        {
            case UnitWeaponry.Shells.Artillery:
            {
                projectileInstance = Instantiate(artilleryShellPrefab, Unit.ShellSpawnLocation.position, Quaternion.identity);
                
                if (projectileInstance is ArtilleryShell artilleryShell) // TODO: Maybe get removed in future
                    artilleryShell.MaxArcHeight = 15.0f; // Safely accessing the specific property
                
                InitializeProjectile(projectileInstance, target);
                projectileInstance.InitializeWeaponryEvents(this);
                projectileInstance.InitializeArmorDamage(_armorDamage);
                
                break;
            }
            case UnitWeaponry.Shells.APShell or UnitWeaponry.Shells.HEShell:
            {
                // Returns, if '_targetUnit' is not spotted anymore and sets '_targetUnit' to null
                if ( ! _targetUnit.IsSpotted)
                {
                    SetTarget(null);
                    return;
                }
                
                projectileInstance = Instantiate(tankShellPrefab, Unit.ShellSpawnLocation.position, Quaternion.identity);
                InitializeProjectile(projectileInstance, target);
                projectileInstance.InitializeWeaponryEvents(this);
                projectileInstance.InitializeArmorDamage(_armorDamage);
                
                break;
            }
        }
    }

    private void InitializeProjectile(Projectile projectileInstance, Unit target)
    {
        projectileInstance.InitialSpeed = _weaponryData.ProjectileSpeed;
        projectileInstance.Target = target;
    }

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
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private bool CanWeaponryAttackTarget(Unit targetUnit) => targetUnit.UnitData.PlayerID != localPlayerID && localArmorDamage[(int)targetUnit.UnitData.Armor] >= 0;

    private bool IsWeaponsCoolDownActive() => CooldownManager.Instance.IsCooldownActive(GetInstanceID());

    public bool CanWeaponryDamageTargetUnit(Unit targetUnit) => localArmorDamage[(int)targetUnit.UnitData.Armor] >= 0;

#endregion
}
