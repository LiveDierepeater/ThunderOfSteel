using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Data")]
    [ExposedScriptableObject]
    public UnitData UnitData;
    [Space(5)]
    
    [Header("Movement Curves")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;
    [Space(5)]
    
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    public bool IsAttacking;
    public bool RandomizeUnitPlayerID;

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
                gameObject.AddComponent<UnitHealth>();
                break;

            case UnitData.Type.Tank:
                gameObject.AddComponent<TankMovement>();
                CreateWeaponry();
                gameObject.AddComponent<UnitHealth>();
                break;

            case UnitData.Type.Truck:
                break;

            default:
                Debug.LogWarning("Unknown Unit-Type: " + UnitData.UnitType);
                break;
        }
    }

    private void Start()
    {
        UnitData.PlayerID = InputManager.Instance.Player.GetInstanceID();
    }

    private void CreateWeaponry()
    {
        foreach (var weaponryData in UnitData.UnitWeaponry)
        {
            Weaponry newWeaponry = Instantiate(UnitData.weaponryPrefab, transform);
            newWeaponry.SetWeaponryData(weaponryData);
        }
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

#endregion

#region Debug

    private void Update()
    {
        if (RandomizeUnitPlayerID)
        {
            UnitData.PlayerID = Random.Range(0, 999999);
            RandomizeUnitPlayerID = false;
        }
    }

#endregion
}
