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

#region Private Fields

    // Interfaces
    private IMovementBehavior _movementBehavior;
    private IAttackBehavior _attackBehavior;

    // Private Fields
    private Unit targetUnit;

#endregion

#region Initializing

    private void Awake()
    {
        SelectionManager.Instance.AvailableUnits.Add(this);

        InitializeUnit();
    }

    private void InitializeUnit()
    {
        switch (DataUnit.UnitType)
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

    public void StopUnit()
    {
        _movementBehavior.StopUnitAtPosition();
    }

#endregion
}
