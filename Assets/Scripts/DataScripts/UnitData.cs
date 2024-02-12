using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Data/Unit")]
public class UnitData : ScriptableObject
{
    public float MaxSpeed;
    public float MaxAcceleration;
    public float MaxDeceleration;
    public float TurnSpeed;
}
