using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TankMovement : UnitSystem, IMovementBehavior
{
    [Header("DEBUG")]
    [SerializeField] private float _time;

#region Internal Fields
    
    // Components
    private NavMeshAgent _agent;
    
    // Functionality Fields
    private float _currentAgentSpeed;
    private float _currentMaxSpeed;
    
    // Stats Fields
    // ReSharper disable once NotAccessedField.Local
    private string _unitName;
    private float _standardSpeed;
    private float _turnSpeed;
    private float _maxAcceleration;
    private float _speedBonusOnRoad;
    private float _speedOnRoad;
    
    private float _stoppingDistance;
    private AnimationCurve _accelerationCurve;
    private AnimationCurve _decelerationCurve;

    private enum MovementStates
    {
        Idle,
        Accelerate,
        Moving,
        Decelerate
    }
    [SerializeField] private MovementStates _currentMovementState = MovementStates.Idle;

    private int _currentAreaIndex;

#endregion
    
#region Initializing

    protected override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();

        Initialize(Unit.UnitData, Unit.accelerationCurve, Unit.decelerationCurve);
    }

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.UnitData.Events.OnAttackUnit += MoveToDestination;
        Unit.UnitData.Events.OnStopUnit += StopUnitAtPosition;
        Unit.UnitData.Events.OnUnitDeath += HandleDeath;
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnAttackUnit -= MoveToDestination;
        Unit.UnitData.Events.OnStopUnit -= StopUnitAtPosition;
    }

    private void OnDestroy()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnAttackUnit -= MoveToDestination;
        Unit.UnitData.Events.OnStopUnit -= StopUnitAtPosition;
    }

    private void Initialize(UnitData data, AnimationCurve accelerationCurve, AnimationCurve decelerationCurve)
    {
        _unitName = data.UnitName;
        _standardSpeed = data.StandardSpeed;
        _turnSpeed = data.TurnSpeed;
        _maxAcceleration = data.MaxAcceleration;
        _stoppingDistance = data.StoppingDistance;
        _speedBonusOnRoad = data.SpeedBonusOnRoad;
        _speedOnRoad = _standardSpeed * _speedBonusOnRoad;
        
        // Functionality Values
        _currentMaxSpeed = _standardSpeed;
        
        // DrivingOnStreets Setup
        if (Unit.UnitData.UnitType == UnitData.Type.Infantry)
            _agent.SetAreaCost(3, 1);
        else
            _agent.SetAreaCost(3, 50);

        // Pasting Agent Values
        _agent.speed = _standardSpeed;
        _agent.angularSpeed = _turnSpeed;
        _agent.acceleration = _maxAcceleration;
        _agent.stoppingDistance = _stoppingDistance;
        
        // Pasting Class Values
        _currentMovementState = MovementStates.Idle;
        _accelerationCurve = accelerationCurve;
        _decelerationCurve = decelerationCurve;
    }
    
#endregion

#region UPDATES

    private void FixedUpdate()
    {
        _currentAgentSpeed = _agent.velocity.magnitude;
        TankMovementBehavior();
    }

    private void TankMovementBehavior()
    {
        // When the tank has a destination
        if (_agent.path.corners.Length <= 1) return;
        
        // Getting values for calculation
        var forwardAmount = 1f;
        var directionToNextPathCorner = (_agent.path.corners[1] - transform.position).normalized;
        var dot = Vector3.Dot(transform.forward, directionToNextPathCorner);
        var angleToDir = Vector3.SignedAngle(transform.forward, directionToNextPathCorner, Vector3.up);
        
        // New forwardAmount when tank needs to turn to nextPathCorner
        if (dot < 0.925f) forwardAmount = 1f * dot;
        
        // The amount the tank has to turn
        var turnAmount = Mathf.Sign(angleToDir) * _turnSpeed * Time.deltaTime * (1f - dot);
        transform.Rotate(Vector3.up, turnAmount);
        
        // Set _agent.speed to new value
        _agent.speed = _currentMaxSpeed * forwardAmount;
    }

    private void HandleTick()
    {
        // Here calculate on Tick-Event.
        HandleMovementState();
        HandleMovementSpeedOnAreas();
    }

#endregion

#region External Called Logic

    public void MoveToDestination(Vector3 newDestination)
    {
        if (IsUnitCloserToDestinationThanStoppingDistance(newDestination))
            _agent.stoppingDistance = 0.5f;

        _agent.SetDestination(newDestination);
        _currentMovementState = MovementStates.Accelerate;
    }

    public void StopUnitAtPosition()
    {
        _agent.SetDestination(_stoppingDistance/2 * _agent.velocity.normalized + transform.position);
        DecelerateNearStoppingDistance();
    }

    private void HandleDeath()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.UnitData.Events.OnAttackUnit -= MoveToDestination;
        Unit.UnitData.Events.OnStopUnit -= StopUnitAtPosition;
    }

#endregion

#region Intern Logic

    #region Movement: Base Logic

    private void HandleMovementState()
    {
        switch (_currentMovementState)
        {
            case MovementStates.Accelerate:
                Accelerate();
                break;

            case MovementStates.Moving:
                Moving();
                break;

            case MovementStates.Decelerate:
                Decelerate();
                break;
        }
    }

    private void Accelerate()
    {
        _agent.speed = _currentMaxSpeed;
        _agent.acceleration = _accelerationCurve.Evaluate(_time) * _maxAcceleration;
        _time += TickSystem.TickRate;

        if (_time >= 1)
        {
            _agent.acceleration = _maxAcceleration;
            _currentMovementState = MovementStates.Moving;
            _time = 1;
        }
            
        if (IsUnitCloserToDestinationThanStoppingDistance(_agent.pathEndPosition))
            DecelerateNearStoppingDistance();
    }

    private void Moving()
    {
        _agent.speed = _currentMaxSpeed;
        if (IsUnitCloserToDestinationThanStoppingDistance(_agent.pathEndPosition))
            DecelerateNearStoppingDistance();
    }

    private void Decelerate()
    {
        _agent.speed = _decelerationCurve.Evaluate(_time) * _currentMaxSpeed;
        _time -= TickSystem.TickRate;

        if (IsUnitStanding())
            ResetUnitMovementValuesToDefault();
    }

    #endregion

    #region Movement: Area-Based Logic

    private void HandleMovementSpeedOnAreas()
    {
        var newUnitArea = GetUnitsCurrentArea();
        
        if (newUnitArea != _currentAreaIndex)
            ChangeUnitsSpeedBasedOnArea(newUnitArea);
    }

    private void ChangeUnitsSpeedBasedOnArea(int newUnitArea)
    {
        // Apply Speed-Changes
        if (newUnitArea - 5 == NavMesh.GetAreaFromName("Road"))
            _currentMaxSpeed = _speedOnRoad;
        else
            _currentMaxSpeed = _standardSpeed;
        
        _currentAreaIndex = newUnitArea;
    }

    #endregion

#endregion

#region Extracted Logic Methods

    private void ResetUnitMovementValuesToDefault()
    {
        _agent.speed = _currentMaxSpeed;
        _time = 0;
        _agent.stoppingDistance = _stoppingDistance;
        _currentMovementState = MovementStates.Idle;
        _agent.ResetPath();
    }

    private void DecelerateNearStoppingDistance()
    {
        // Return, if unit already came to stop
        if (IsUnitStanding()) return;
        
        // When the current speed of the '_agent' is >= than the distance to the final destination, decelerate Unit

        if (_currentAgentSpeed >= Vector3.Distance(transform.position, _agent.pathEndPosition))
            _currentMovementState = MovementStates.Decelerate;
    }

#endregion

#region Extracted Return Methods

    private bool IsUnitCloserToDestinationThanStoppingDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < _agent.stoppingDistance * 2;
    }

    private bool IsUnitStanding()
    {
        return _currentAgentSpeed < 0.25f;
    }

    private int GetUnitsCurrentArea()
    {
        _agent.SamplePathPosition(NavMesh.AllAreas, 1.0f, out var hit);
        return hit.mask;
    }

#endregion

    /*private void OnDrawGizmos()
    {
        if (_agent == null || _agent.path == null)
            return;

        // Get the corners of the agent's path
        Vector3[] corners = _agent.path.corners;

        // Draw a sphere at each corner
        for (int i = 0; i < corners.Length; i++)
        {
            // Set the gizmo color
            if (i == 0) Gizmos.color = Color.yellow;
            else Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(corners[i], 1.5f);

            // Draw lines between the corners
            if (i < corners.Length - 1)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }*/
}
