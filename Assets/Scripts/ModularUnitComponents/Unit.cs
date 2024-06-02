using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    public UnitEvents Events;
    public event Action OnInitializeChip;
    public event Action OnUnitDeathInternal;
    
    [Header("Communication")]
    public UnitCommands CurrentUnitCommand;
    public enum UnitCommands
    {
        Idle,
        Move,
        Attack
    }
    
    private bool IsUnitDead { get; set; }
    
    [Header("Data")]
    //[ExposedScriptableObject]
    public UnitData UnitData;
    public Transform Turret;
    public Transform ShellSpawnLocation;
    public Unit TargetUnit;
    [HideInInspector] public Transform Mesh;
    [HideInInspector] public SphereCollider Collider;
    [HideInInspector] public Color PlayerColor;
    [Space(5)]
    
    [Header("Movement Curves")]
    [HideInInspector] public TankMovement TankMovement;
    [HideInInspector] public InfantryMovement InfantryMovement;
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;

    [Space(5)]
    [Header("Spotting System")]
    [HideInInspector] public USpottingSystem USpottingSystem;
    [HideInInspector] public WeaponryHandler WeaponryHandler;
    [HideInInspector] public UHealth UHealth;
    public Unit SpottingUnit;
    public bool IsSpotted;
    
    [Space(10)]
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    private Color selectionSpriteColor;
    public bool IsAttacking; // Could be removed in future
    public int UnitPlayerID;

#region Initializing

    private void Awake()
    {
        // Initialize the Events struct
        Events = new UnitEvents();
        
        InitializeUnit();
    }

    private void InitializeUnit()
    {
        switch (UnitData.UnitType)
        {
            case UnitData.Type.Infantry:
                InfantryMovement = gameObject.AddComponent<InfantryMovement>();
                UHealth =gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Tank:
                TankMovement = gameObject.AddComponent<TankMovement>();
                UHealth =gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Truck:
                InfantryMovement = gameObject.AddComponent<InfantryMovement>();
                UHealth =gameObject.AddComponent<UHealth>();
                break;
            
            default:
                Debug.LogWarning("Unknown Unit-Type: " + UnitData.UnitType);
                break;
        }
        
        Mesh = transform.Find("Mesh");
        Collider = GetComponent<SphereCollider>();
    }

    private void CreateWeaponry()
    {
        foreach (var weaponryData in UnitData.UnitWeaponry)
        {
            Weaponry newWeaponry = Instantiate(UnitData.weaponryPrefab, transform);
            newWeaponry.InitializeWeaponry(weaponryData);
        }
    }

    private void Start()
    {
        UnitPlayerID = InputManager.Instance.Player.GetInstanceID();
        WeaponryHandler = GetComponent<WeaponryHandler>();
        
        SelectionManager.Instance.AvailableUnits.Add(this);
        UnitManager.Instance.AddUnit(this, UnitPlayerID);
        
        InitializeSpritePlayerColor();
        OnInitializeChip?.Invoke();
        
        UnitDeathInitialization();
    }

    private void InitializeSpritePlayerColor()
    {
        if (UnitPlayerID == InputManager.Instance.Player.GetInstanceID())
        {
            selectionSpriteColor = InputManager.Instance.Player.PlayerColor;
            selectionSprite.color = selectionSpriteColor;
            PlayerColor = selectionSpriteColor;
        }
        else if (CompareTag("AI"))
        {
            selectionSpriteColor = InputManager.Instance.Player.EnemyColor;
            selectionSprite.color = selectionSpriteColor;
            PlayerColor = selectionSpriteColor;
        }
        else if (CompareTag("Ally"))
        {
            selectionSpriteColor = InputManager.Instance.Player.AllyColor;
            selectionSprite.color = selectionSpriteColor;
            PlayerColor = selectionSpriteColor;
        }
    }

    private void UnitDeathInitialization()
    {
        // Unit Death Setup
        Events.OnUnitDeath += SetUnitDead;
        TickManager.Instance.TickSystem.OnTickEnd += DestroyUnit;
    }

    private void OnDestroy()
    {
        UnitManager.Instance.RemoveUnit(this, UnitPlayerID);
        Events.OnUnitDeath -= SetUnitDead;
        TickManager.Instance.TickSystem.OnTickEnd -= DestroyUnit;
    }

#endregion

#region External Called Logic

    public void OnSelected() => selectionSprite.color = Color.white;

    public void OnDeselected() => selectionSprite.color = selectionSpriteColor;

    public void CommandToDestination(Vector3 newDestination)
    {
        Events.OnCommandToDestination?.Invoke(newDestination);
        TargetUnit = null;
        CurrentUnitCommand = UnitCommands.Move;
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        CurrentUnitCommand = UnitCommands.Attack;
        TargetUnit = newUnitTarget;
        TargetUnit.Events.OnUnitDeath += RemoveTarget;
        Events.OnNewTargetUnit?.Invoke(newUnitTarget);
    }

    public void RemoveTarget()
    {
        Events.OnNewTargetUnit?.Invoke(null);
        
        if (TargetUnit is not null)
        {
            TargetUnit.Events.OnUnitDeath -= RemoveTarget;
            TargetUnit = null;
        }
        
        CurrentUnitCommand = UnitCommands.Idle;
    }

    private void SetUnitDead() => IsUnitDead = true;

    private void DestroyUnit()
    {
        if ( ! IsUnitDead) return;
        
        OnUnitDeathInternal?.Invoke();
        Mesh.gameObject.SetActive(false);
        Collider.enabled = false;
        StartCoroutine(KillUnit());
    }

#endregion

    private IEnumerator KillUnit()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
        print(UnitData.UnitName + " is destroyed!");
    }

#region Debug

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && selectionSprite.color == Color.white) RandomizePlayerID();
    }

    private void RandomizePlayerID()
    {
        Vector2Int currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
            
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(this, currentHashKey);
            
        int newPlayerID = Random.Range(0, 999999);
        UnitPlayerID = newPlayerID;
        
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(this, currentHashKey);
    }

#endregion
}
