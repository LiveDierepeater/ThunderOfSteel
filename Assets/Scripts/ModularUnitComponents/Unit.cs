using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    public event Action OnInitializeChip;
    public event Action OnUnitDeath;
    
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
        SelectionManager.Instance.AvailableUnits.Add(this);

        InitializeUnit();
    }

    private void InitializeUnit()
    {
        UnitData = Instantiate(UnitData);
        UnitData.InstanceID = Mathf.Abs(GetInstanceID()) * 1000 + 1;
        
        switch (UnitData.UnitType)
        {
            case UnitData.Type.Infantry:
                InfantryMovement = gameObject.AddComponent<InfantryMovement>();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Tank:
                TankMovement = gameObject.AddComponent<TankMovement>();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Truck:
                InfantryMovement = gameObject.AddComponent<InfantryMovement>();
                gameObject.AddComponent<UHealth>();
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
            newWeaponry.SetWeaponryData(weaponryData);
        }
    }

    private void Start()
    {
        UnitData.PlayerID = InputManager.Instance.Player.GetInstanceID();
        UnitPlayerID = UnitData.PlayerID;
        UnitManager.Instance.AddUnit(this, UnitPlayerID);
        
        InitializeSpritePlayerColor();
        OnInitializeChip?.Invoke();
        
        UnitDeathInitialization();
    }

    private void InitializeSpritePlayerColor()
    {
        if (UnitData.PlayerID == InputManager.Instance.Player.GetInstanceID())
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
        UnitData.Events.OnUnitDeath += SetUnitDead;
        TickManager.Instance.TickSystem.OnTickEnd += DestroyUnit;
    }

    private void OnDestroy()
    {
        UnitManager.Instance.RemoveUnit(this, UnitPlayerID);
        UnitData.Events.OnUnitDeath -= SetUnitDead;
        TickManager.Instance.TickSystem.OnTickEnd -= DestroyUnit;
    }

#endregion

#region External Called Logic

    public void OnSelected() => selectionSprite.color = Color.white;

    public void OnDeselected() => selectionSprite.color = selectionSpriteColor;

    public void CommandToDestination(Vector3 newDestination)
    {
        UnitData.Events.OnCommandToDestination?.Invoke(newDestination);
        TargetUnit = null;
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Move;
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Attack;
        TargetUnit = newUnitTarget;
        TargetUnit.UnitData.Events.OnUnitDeath += RemoveTarget;
        UnitData.Events.OnNewTargetUnit?.Invoke(newUnitTarget);
    }

    public void RemoveTarget()
    {
        UnitData.Events.OnNewTargetUnit?.Invoke(null);
        
        if (TargetUnit is not null)
        {
            TargetUnit.UnitData.Events.OnUnitDeath -= RemoveTarget;
            TargetUnit = null;
        }
        
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Idle;
    }

    private void SetUnitDead() => IsUnitDead = true;

    private void DestroyUnit()
    {
        if (!IsUnitDead) return;
        
        OnUnitDeath?.Invoke();
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
        UnitData.PlayerID = newPlayerID;
            
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(this, currentHashKey);
    }

#endregion
}
