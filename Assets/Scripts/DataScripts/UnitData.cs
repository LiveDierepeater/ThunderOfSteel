using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Data/Unit")]
public class UnitData : ScriptableObject
{
    public string UnitName;
    public float MaxSpeed;
    public float TurnSpeed;
    public float MaxAcceleration;
    public float StoppingDistance;
    
    public float SpeedBonusOnRoad;
    public float AttackRange;
    
    public enum Type
    {
        Infantry,
        Tank,
        Truck
    }
    public Type UnitType;
}
