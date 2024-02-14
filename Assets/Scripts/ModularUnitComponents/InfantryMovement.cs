using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class InfantryMovement : MonoBehaviour, IMovementBehavior
{
    private NavMeshAgent agent;
    
    private string _unitName;
    private float _maxSpeed;
    private float _turnSpeed;
    private float _maxAcceleration;
    private float _stoppingDistance;

    [Header("Movement")]
    private AnimationCurve accelerationCurve;
    private AnimationCurve decelerationCurve;
    [SerializeField] private float time;

    private enum States
    {
        Idle,
        MoveToDestination,
    }
    [SerializeField] private States currentState = States.Idle;

    private enum MovementStates
    {
        Idle,
        Accelerate,
        Moving,
        Decelerate
    }
    [SerializeField] private MovementStates currentMovementState = MovementStates.Idle;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
    }

    public void Initialize(UnitData data, AnimationCurve accelerationCurve, AnimationCurve decelerationCurve)
    {
        _unitName = data.UnitName;
        _maxSpeed = data.MaxSpeed;
        _turnSpeed = data.TurnSpeed;
        _maxAcceleration = data.MaxAcceleration;
        _stoppingDistance = data.StoppingDistance;

        // Pasting Agent Values
        agent.speed = _maxSpeed;
        agent.angularSpeed = _turnSpeed;
        agent.acceleration = _maxAcceleration;
        agent.stoppingDistance = _stoppingDistance;
        
        // Pasting Class Values
        currentState = States.Idle;
        currentMovementState = MovementStates.Idle;
        this.accelerationCurve = accelerationCurve;
        this.decelerationCurve = decelerationCurve;
    }

    private bool IsUnitCloserThanStoppingDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < agent.stoppingDistance;
    }

    private bool IsUnitStanding()
    {
        return agent.velocity.normalized.magnitude < 0.5f;
    }

    private void Accelerate()
    {
        agent.speed = _maxSpeed;
        agent.acceleration = accelerationCurve.Evaluate(time) * _maxAcceleration;
        time += Time.deltaTime * 30;

        if (time >= 1)
        {
            agent.acceleration = _maxAcceleration;
            currentMovementState = MovementStates.Moving;
        }
    }

    private void Decelerate()
    {
        agent.speed = decelerationCurve.Evaluate(time)  * _maxSpeed;
        time += Time.deltaTime;

        if (time >= 1)
        {
            agent.speed = _maxSpeed;
            currentMovementState = MovementStates.Idle;
            currentState = States.Idle;
        }
    }
    
    public void MoveToDestination(Vector3 newDestination)
    {
        if (IsUnitCloserThanStoppingDistance(newDestination))
            agent.stoppingDistance = 0.5f;
        
        agent.SetDestination(newDestination);
        currentState = States.MoveToDestination;

        currentMovementState = MovementStates.Accelerate;
        time = 0;
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
                    agent.stoppingDistance = _stoppingDistance;
                    time = 0;
                    currentMovementState = MovementStates.Decelerate;

                    if (IsUnitStanding())
                    {
                        agent.speed = _maxSpeed;
                        currentMovementState = MovementStates.Idle;
                        currentState = States.Idle;
                    }
                }
                break;
        }
    }

    private void HandleTick()
    {
        // Here calculate on Tick-Event.
        HandleMovementState();
    }

    private void HandleMovementState()
    {
        switch (currentMovementState)
        {
            case MovementStates.Accelerate:
                Accelerate();
                break;
            
            case MovementStates.Decelerate:
                Decelerate();
                break;
        }
    }
}
