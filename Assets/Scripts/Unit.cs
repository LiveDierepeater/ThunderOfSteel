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

    public AnimationCurve movementCurve;
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
                    currentState = States.Idle;
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

    public void Accelerate()
    {
        print("Accelerate!");
        
        agent.acceleration = movementCurve.Evaluate(time) * MaxAcceleration;
        time += Time.deltaTime * 30;

        if (time > 1)
        {
            agent.acceleration = MaxAcceleration;
            currentMovementState = MovementStates.Moving;
        }
    }

    public void Decelerate()
    {
        print("Decelerate!");
        
        agent.acceleration = movementCurve.Evaluate(time)  * MaxDeceleration;
        time += Time.deltaTime;

        if (time > 1)
        {
            agent.acceleration = MaxDeceleration;
            currentMovementState = MovementStates.Moving;
        }
    }
}
