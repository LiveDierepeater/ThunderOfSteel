using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private bool IsUnitDead { get; set; }
    
    [Header("Data")]
    [ExposedScriptableObject]
    public UnitData UnitData;
    [HideInInspector] public Transform Mesh;
    [HideInInspector] public SphereCollider Collider;
    [Space(5)]
    
    [Header("Movement Curves")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;

    [Space(5)]
    
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    private Color selectionSpriteColor;
    public Transform ShellSpawnLocation;
    public bool IsAttacking; // Could be removed in future
    public int UnitPlayerID;

    [Space(10)]
    [HideInInspector] public USpottingSystem USpottingSystem;
    public Unit SpottingUnit;
    public bool IsSpotted;

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
                gameObject.AddComponent<InfantryMovement>();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Tank:
                gameObject.AddComponent<TankMovement>();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                CreateWeaponry();
                break;
            
            case UnitData.Type.Truck:
                gameObject.AddComponent<InfantryMovement>();
                gameObject.AddComponent<UHealth>();
                break;
            
            default:
                Debug.LogWarning("Unknown Unit-Type: " + UnitData.UnitType);
                break;
        }
        
        ShellSpawnLocation = transform.Find("ShellSpawnLocation");
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
        UnitDeathInitialization();
    }

    private void InitializeSpritePlayerColor()
    {
        if (UnitData.PlayerID == InputManager.Instance.Player.GetInstanceID())
        {
            selectionSpriteColor = InputManager.Instance.Player.PlayerColor;
            selectionSprite.color = selectionSpriteColor;
        }
        else if (CompareTag("AI"))
        {
            selectionSpriteColor = Color.red;
            selectionSprite.color = selectionSpriteColor;
        }
        else if (CompareTag("Ally"))
        {
            selectionSpriteColor = Color.green;
            selectionSprite.color = selectionSpriteColor;
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
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Move;
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Attack;
        UnitData.Events.OnNewTargetUnit?.Invoke(newUnitTarget);
    }

    public void RemoveTarget()
    {
        UnitData.Events.OnNewTargetUnit?.Invoke(null);
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Idle;
    }
    
    private void SetUnitDead() => IsUnitDead = true;

    private void DestroyUnit()
    {
        if (!IsUnitDead) return;
        
        //UnitManager.Instance.RemoveUnit(this, UnitPlayerID);
        //transform.gameObject.SetActive(false);
            
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
