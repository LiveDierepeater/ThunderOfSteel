using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Data")]
    public UnitData Data;
    [Space(5)]
    
    [Header("Movement")]
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;
    [Space(5)]
    
    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    
    // Interfaces
    private IMovementBehavior _movementBehavior;
    private IAttackBehavior _attackBehavior;
    
    // Private Fields
    private Unit targetUnit;
    
    private void Awake()
    {
        SelectionManager.Instance.AvailableUnits.Add(this);
        
        InitializeUnit();
    }
    
    private void InitializeUnit()
    {
        switch (Data.UnitType)
        {
            case UnitData.Type.Infantry:
                _movementBehavior = gameObject.AddComponent<InfantryMovement>();
                _attackBehavior = gameObject.AddComponent<InfantryCombat>();
                break;
            
            case UnitData.Type.Tank:
                break;
            
            case UnitData.Type.Truck:
                break;
            
            default:
                Debug.LogWarning("Unknown Unit-Type: " + Data.UnitType);
                break;
        }

        _movementBehavior.Initialize(Data, accelerationCurve, decelerationCurve);
        _attackBehavior.Initialize(Data);
    }
    
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
        _movementBehavior.MoveToDestination(newDestination);
    }

    public void CommandToAttack(Unit newUnitTarget)
    {
        _attackBehavior.SetTarget(newUnitTarget);
    }

    public void RemoveTarget()
    {
        _attackBehavior.SetTarget(null);
    }
}
