using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Data/Unit")]
public class UnitData : ScriptableObject
{
    public int InstanceID;
    public string UnitName;
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
    public Armors Armor;
    public int MaxHealth;
    
    [ExposedScriptableObject]
    public UnitWeaponry[] UnitWeaponry;
    
    public enum Type
    {
        Infantry,
        Tank,
        Truck
    }
    public Type UnitType;
    
    public enum UnitCommands
    {
        Idle,
        Move,
        Attack
    }
    public UnitCommands CurrentUnitCommand;

    public UnitEvents Events;
}
