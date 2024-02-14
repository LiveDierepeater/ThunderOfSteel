using UnityEngine;

public class InfantryMovement : UnitMovement
{
    private float _maxSpeed;

    private void Start()
    {
        print(_maxSpeed);
    }

    public void Initialize(UnitData data)
    {
        _maxSpeed = data.MaxSpeed;
    }
    
    public override void MoveToDestination(Vector3 destination)
    {
        
    }
}
