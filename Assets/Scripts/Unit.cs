using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer selectionSprite;
    
    private NavMeshAgent agent;
    
    public float MaxSpeed;
    public float MaxAcceleration;
    public float MaxDeceleration;
    public float TurnSpeed;
    public string UnitName;
    public float SpeedBonusOnRoad;
    public float StoppingDistance;

    private enum States
    {
        Idle,
        MoveToDestination,
    }
    [SerializeField] private States currentState = States.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        SelectionManager.Instance.AvailableUnits.Add(this);
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        
        InitializeClassValues();
    }

    public void OnSelected()
    {
        selectionSprite.gameObject.SetActive(true);
    }

    public void OnDeselected()
    {
        selectionSprite.gameObject.SetActive(false);
    }
    
    public void MoveToDestination(Vector3 newDestination)
    {
        if (IsUnitCloserThanStoppingDistance(newDestination))
            agent.stoppingDistance = 0.5f;
        
        agent.SetDestination(newDestination);
        currentState = States.MoveToDestination;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case States.Idle:
                break;
            
            case States.MoveToDestination:
                if (IsUnitCloserThanStoppingDistance(agent.pathEndPosition))
                {
                    agent.stoppingDistance = StoppingDistance;
                    currentState = States.Idle;
                }
                break;
        }
    }

    private void HandleTick()
    {
        // Here calculate on Tick-Event.
    }

    private void InitializeClassValues()
    {
        // Renaming Unit
        gameObject.name = UnitName + "_" + GetInstanceID();
        
        // Pasting Agent Values
        agent.speed = MaxSpeed;
        agent.angularSpeed = TurnSpeed;
        agent.acceleration = MaxAcceleration;
        agent.stoppingDistance = StoppingDistance;
        agent.autoBraking = false;
        
        // Pasting Class Values
        currentState = States.Idle;
    }

    private bool IsUnitCloserThanStoppingDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < agent.stoppingDistance;
    }
}
