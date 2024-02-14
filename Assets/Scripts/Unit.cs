using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    [Space(5)]
    
    private NavMeshAgent agent;
    
    [Header("NavMeshAgent")]
    public float MaxSpeed;
    public float TurnSpeed;
    public float MaxAcceleration;
    public float StoppingDistance;
    [Space(5)]
    
    [Header("Unit Description")]
    public string UnitName;
    public float SpeedBonusOnRoad;
    [Space(5)]

    [Header("Movement")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;
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
        SelectionManager.Instance.AvailableUnits.Add(this);
        
        InitializeClassValues();
    }

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
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
                    agent.stoppingDistance = StoppingDistance;
                    time = 0;
                    currentMovementState = MovementStates.Decelerate;

                    if (IsUnitStanding())
                    {
                        agent.speed = MaxSpeed;
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

    private void InitializeClassValues()
    {
        // Renaming Unit
        gameObject.name = UnitName + "_" + GetInstanceID();
        
        // Pasting Agent Values
        agent.speed = MaxSpeed;
        agent.angularSpeed = TurnSpeed;
        agent.acceleration = MaxAcceleration;
        agent.stoppingDistance = StoppingDistance;
        
        // Pasting Class Values
        currentState = States.Idle;
        currentMovementState = MovementStates.Idle;
    }

    private bool IsUnitCloserThanStoppingDistance(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) < agent.stoppingDistance;
    }

    private bool IsUnitStanding()
    {
        return agent.velocity.normalized.magnitude < 0.5f;
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

    private void Accelerate()
    {
        agent.speed = MaxSpeed;
        agent.acceleration = accelerationCurve.Evaluate(time) * MaxAcceleration;
        time += Time.deltaTime * 30;

        if (time >= 1)
        {
            agent.acceleration = MaxAcceleration;
            currentMovementState = MovementStates.Moving;
        }
    }

    private void Decelerate()
    {
        agent.speed = decelerationCurve.Evaluate(time)  * MaxSpeed;
        time += Time.deltaTime;

        if (time >= 1)
        {
            agent.speed = MaxSpeed;
            currentMovementState = MovementStates.Idle;
            currentState = States.Idle;
        }
    }
}
