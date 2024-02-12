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

    public enum States
    {
        Idle,
        MoveToDestination,
        Flee
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        SelectionManager.Instance.AvailableUnits.Add(this);
        TickManager.Instance.TickSystem.OnTick += HandleTick;
    }

    private void Start()
    {
        InitializeAgentValues();
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
        agent.SetDestination(newDestination);
    }

    private void HandleTick()
    {
        print("Tick in Unit");
    }
    
    private void InitializeAgentValues()
    {
        agent.speed = MaxSpeed;
        agent.acceleration = MaxAcceleration;
        agent.angularSpeed = TurnSpeed;
        gameObject.name = UnitName + "_" + GetInstanceID();
    }
}
