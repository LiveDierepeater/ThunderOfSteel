using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private bool IsUnitDead { get; set; }
    
    [Header("Data")]
    [ExposedScriptableObject]
    public UnitData UnitData;
    public Transform Mesh;
    [Space(5)]
    
    [Header("Movement Curves")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;
    [Space(5)]
    
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    public Transform ShellSpawnLocation;
    public bool IsAttacking; // Could be removed in future
    public bool RandomizeUnitPlayerID;
    public int UnitPlayerID;

    [Space(10)]
    public USpottingSystem USpottingSystem;
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
                CreateWeaponry();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
                break;
            
            case UnitData.Type.Tank:
                gameObject.AddComponent<TankMovement>();
                CreateWeaponry();
                gameObject.AddComponent<UHealth>();
                USpottingSystem = gameObject.AddComponent<USpottingSystem>();
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
    }

    private void Start()
    {
        UnitData.PlayerID = InputManager.Instance.Player.GetInstanceID();
        UnitPlayerID = UnitData.PlayerID;
        //UnitManager.Instance.AddUnit(this, UnitPlayerID);
        UnitDeathInitialization();
    }

    private void CreateWeaponry()
    {
        foreach (var weaponryData in UnitData.UnitWeaponry)
        {
            Weaponry newWeaponry = Instantiate(UnitData.weaponryPrefab, transform);
            newWeaponry.SetWeaponryData(weaponryData);
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
        UnitData.Events.OnUnitDeath -= SetUnitDead;
        TickManager.Instance.TickSystem.OnTickEnd -= DestroyUnit;
    }

#endregion

#region External Called Logic

    public void OnSelected()
    {
        selectionSprite.gameObject.SetActive(true);
    }

    public void OnDeselected()
    {
        selectionSprite.gameObject.SetActive(false);
    }

    public void CommandToDestination(Vector3 newDestination)
    {
        UnitData.Events.OnAttackUnit?.Invoke(newDestination);
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Move;
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        UnitData.Events.OnNewTargetUnit?.Invoke(newUnitTarget);
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Attack;
    }

    public void RemoveTarget()
    {
        UnitData.Events.OnNewTargetUnit?.Invoke(null);
        UnitData.CurrentUnitCommand = UnitData.UnitCommands.Idle;
    }
    
    private void SetUnitDead()
    {
        IsUnitDead = true;
    }

    private void DestroyUnit()
    {
        if (IsUnitDead)
        {
            //UnitManager.Instance.RemoveUnit(this, UnitPlayerID);
            //transform.gameObject.SetActive(false);
            
            Mesh.gameObject.SetActive(false);
            StartCoroutine(KillUnit());
        }
    }

#endregion

#region Debug

    private void Update()
    {
        if (RandomizeUnitPlayerID) RandomizePlayerID();
        if (Input.GetKeyDown(KeyCode.K) && selectionSprite.gameObject.activeSelf) RandomizePlayerID();
    }

    private void RandomizePlayerID()
    {
        Vector2Int currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
            
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(this, currentHashKey);
            
        int newPlayerID = Random.Range(0, 999999);
        UnitPlayerID = newPlayerID;
        UnitData.PlayerID = newPlayerID;
            
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(this, currentHashKey);

        RandomizeUnitPlayerID = false;
    }

    private IEnumerator KillUnit()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

#endregion
}
