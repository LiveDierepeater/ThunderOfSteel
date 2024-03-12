using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Data/Unit")]
public class UnitData : ScriptableObject
{
    [Header("Base")]
    public int InstanceID;
    public int PlayerID;
    public string UnitName;
    
    public enum Factions
    {
        UnitedStates,
        Germany,
        Russia
    }
    public Factions Faction;

    public string Class;

    public enum WarModes
    {
        _1939,
        _1942,
        _1945
    }
    public WarModes WarMode;
    
    public enum Type
    {
        Infantry,
        Tank,
        Truck
    }
    public Type UnitType;

    [Header("Production")]
    
    public int Cost;

    public enum ProductionBase
    {
        HQ,
        Barracks,
        AAA,
        TankBase,
        AntiTankBase,
        Airfield,
        Prototype
    }
    public ProductionBase ProducedBy;
    
    [Header("Movement")]
    
    public float StandardSpeed;
    public float TurnSpeed;
    public float MaxAcceleration;
    public float StoppingDistance;
    
    public float SpeedBonusOnRoad;
    
    public enum Armors
    {
        Infantry,
        Truck,
        Building,
        Level_01,
        Level_02,
        Level_03,
        Level_04,
        Level_05,
        Air
    }
    [Header("Resistance")]
    public Armors Armor;
    public int MaxHealth;

    [Header("Combat")]
    public UnitCombat UnitCombatPrefab;
    [ExposedScriptableObject]
    public UnitWeaponry[] UnitWeaponry;
    
    
    public enum UnitCommands
    {
        Idle,
        Move,
        Attack
    }
    [Header("Communication")]
    public UnitCommands CurrentUnitCommand;

    public UnitEvents Events;
}
