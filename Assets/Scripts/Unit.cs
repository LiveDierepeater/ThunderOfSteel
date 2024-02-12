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

    private void Update()
    {
        foreach (Vector3 pathCorner in agent.path.corners)
        {
            DrawSphere(pathCorner, 1f, Color.yellow);
        }
    }

    private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);
    
    private static Vector4[] MakeUnitSphere(int len)
    {
        Debug.Assert(len > 2);
        var v = new Vector4[len * 3];
        for (int i = 0; i < len; i++)
        {
            var f = i / (float)len;
            float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
            float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
            v[0 * len + i] = new Vector4(c, s, 0, 1);
            v[1 * len + i] = new Vector4(0, c, s, 1);
            v[2 * len + i] = new Vector4(s, 0, c, 1);
        }
        return v;
    }
    
    public static void DrawSphere(Vector4 pos, float radius, Color color)
    {
        Vector4[] v = s_UnitSphere;
        int len = s_UnitSphere.Length / 3;
        for (int i = 0; i < len; i++)
        {
            var sX = pos + radius * v[0 * len + i];
            var eX = pos + radius * v[0 * len + (i + 1) % len];
            var sY = pos + radius * v[1 * len + i];
            var eY = pos + radius * v[1 * len + (i + 1) % len];
            var sZ = pos + radius * v[2 * len + i];
            var eZ = pos + radius * v[2 * len + (i + 1) % len];
            Debug.DrawLine(sX, eX, color);
            Debug.DrawLine(sY, eY, color);
            Debug.DrawLine(sZ, eZ, color);
        }
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
