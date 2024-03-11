using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Data")]
    [ExposedScriptableObject]
    public UnitData DataUnit;
    [Space(5)]
    
    [Header("Movement")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;
    [Space(5)]
    
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    public bool IsAttacking;

#region Initializing

    private void Awake()
    {
        SelectionManager.Instance.AvailableUnits.Add(this);

        InitializeUnit();
    }

    private void InitializeUnit()
    {
        DataUnit = Instantiate(DataUnit);
        DataUnit.InstanceID = Mathf.Abs(GetInstanceID()) * 1000 + 1;
        
        switch (DataUnit.UnitType)
        {
            case UnitData.Type.Infantry:
                gameObject.AddComponent<InfantryMovement>();
                gameObject.AddComponent<UnitCombat>();
                break;

            case UnitData.Type.Tank:
                gameObject.AddComponent<TankMovement>();
                gameObject.AddComponent<UnitCombat>();
                break;

            case UnitData.Type.Truck:
                break;

            default:
                Debug.LogWarning("Unknown Unit-Type: " + DataUnit.UnitType);
                break;
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
        DataUnit.Events.OnAttackUnit?.Invoke(newDestination);
        DataUnit.CurrentUnitCommand = UnitData.UnitCommands.Move;
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        DataUnit.Events.OnNewTargetUnit?.Invoke(newUnitTarget);
        DataUnit.CurrentUnitCommand = UnitData.UnitCommands.Attack;
    }

    public void RemoveTarget()
    {
        DataUnit.Events.OnNewTargetUnit?.Invoke(null);
        DataUnit.CurrentUnitCommand = UnitData.UnitCommands.Idle;
    }

#endregion
}
