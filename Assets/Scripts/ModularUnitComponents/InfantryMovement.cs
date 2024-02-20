using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class InfantryMovement : MonoBehaviour, IMovementBehavior
{
    [Header("DEBUG")]
    [SerializeField] private float _time;
    
    // Components
    private NavMeshAgent _agent;
    
    // Private Fields
    private string _unitName;
    private float _maxSpeed;
    private float _turnSpeed;
    private float _maxAcceleration;
    private float _speedBonusOnRoad;
    
    private float _stoppingDistance;
    private AnimationCurve _accelerationCurve;
    private AnimationCurve _decelerationCurve;

    private enum States
    {
        Idle,
        MoveToDestination,
    }
    [SerializeField] private States _currentState = States.Idle;

    private enum MovementStates
    {
        Idle,
        Accelerate,
        Moving,
        Decelerate
    }
    [SerializeField] private MovementStates _currentMovementState = MovementStates.Idle;
    
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
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
        _speedBonusOnRoad = data.SpeedBonusOnRoad;

        // Pasting Agent Values
        _agent.speed = _maxSpeed;
        _agent.angularSpeed = _turnSpeed;
        _agent.acceleration = _maxAcceleration;
        _agent.stoppingDistance = _stoppingDistance;
        
        // Pasting Class Values
        _currentState = States.Idle;
        _currentMovementState = MovementStates.Idle;
        this._accelerationCurve = accelerationCurve;
        this._decelerationCurve = decelerationCurve;
    }

    private bool IsUnitCloserThanStoppingDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < _agent.stoppingDistance;
    }

    private bool IsUnitStanding()
    {
        return _agent.velocity.normalized.magnitude < 0.5f;
    }

    private void Accelerate()
    {
        _agent.speed = _maxSpeed;
        _agent.acceleration = _accelerationCurve.Evaluate(_time) * _maxAcceleration;
        _time += TickSystem.TickRate;
        
        if (_time >= 1)
        {
            _agent.acceleration = _maxAcceleration;
            _currentMovementState = MovementStates.Moving;
        }
    }

    private void Decelerate()
    {
        _agent.speed = _decelerationCurve.Evaluate(_time)  * _maxSpeed;
        _time += TickSystem.TickRate;

        if (_time >= 1)
        {
            _agent.speed = _maxSpeed;
            _currentMovementState = MovementStates.Idle;
            _currentState = States.Idle;
        }
    }
    
    public void MoveToDestination(Vector3 newDestination)
    {
        if (IsUnitCloserThanStoppingDistance(newDestination))
            _agent.stoppingDistance = 0.5f;
        
        _agent.SetDestination(newDestination);
        _currentState = States.MoveToDestination;

        _currentMovementState = MovementStates.Accelerate;
        _time = 0;
    }

    private void FixedUpdate()
    {
        switch (_currentState)
        {
            case States.Idle:
                break;
            
            case States.MoveToDestination:
                if (IsUnitCloserThanStoppingDistance(_agent.pathEndPosition))
                {
                    _agent.stoppingDistance = _stoppingDistance;
                    _time = 0;
                    _currentMovementState = MovementStates.Decelerate;

                    if (IsUnitStanding())
                    {
                        _agent.speed = _maxSpeed;
                        _currentMovementState = MovementStates.Idle;
                        _currentState = States.Idle;
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
        switch (_currentMovementState)
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
