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
    
    private IMovementBehavior _movementBehavior;

    [Header("Debug")]
    [SerializeField] private SpriteRenderer selectionSprite;
    
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
}
