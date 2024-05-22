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
    
    public Type UnitType;
    public enum Type
    {
        Infantry,
        Tank,
        Truck
    }

    [Header("Production")]
    public int Cost;
    public ProductionBase ProducedBy;
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
    
    [Header("Movement")]
    
    public float StandardSpeed;
    public float TurnSpeed;
    public float MaxAcceleration;
    public float StoppingDistance;
    public float FleeSpeed;
    public float SpeedBonusOnRoad;
    
    [Header("Resistance")]
    public Armors Armor;
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
    public int MaxHealth;

    [Header("Combat")]
    public Weaponry weaponryPrefab;
    [ExposedScriptableObject]
    public UnitWeaponry[] UnitWeaponry;

    [Header("Spotting")]
    public float SpottingRange;

    public ChipType Chip;
    public enum ChipType
    {
        Small,
        Big
    }
    
    [Header("Communication")]
    public UnitCommands CurrentUnitCommand;
    public enum UnitCommands
    {
        Idle,
        Move,
        Attack
    }

    public UnitEvents Events;
}
