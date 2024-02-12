using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    private NavMeshAgent agent;
    
    public float MaxSpeed;
    public float MaxAcceleration;
    public float MaxDeceleration;
    public float TurnSpeed;
    public string UnitName;
    public float SpeedBonusOnRoad;
    
    public enum States
    {
        Idle = 0,
        MoveToDestination = 1,
        Flee = 2
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
}
