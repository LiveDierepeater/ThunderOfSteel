using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGamePlayerController : MonoBehaviour
{
    public GameObject moveToSpritePrefab;
    
    private new Camera camera;
    
    [SerializeField] private RectTransform selectionBox;
    private LayerMask _unitsLayerMask;
    private LayerMask _terrainLayerMask;
    private LayerMask _woodsLayerMask;
    private LayerMask _interactableLayerMask;
    [SerializeField] private float dragDelay = 0.1f;

    [SerializeField, Range(0, 10)] private int _unitWidth = 10;
    [SerializeField] private float _unitSpacing = 20f;

    [SerializeField] private GameObject selectedUnitGameObject;

#region Internal Fields

    private bool isDragging;
    private float mouseDownTime;
    private Vector2 startMousePosition;
    
    private KeyCode lastHitKey;
    private KeyCode currentHitKey;

#endregion

#region Initializing

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
        selectionBox = GameObject.FindGameObjectWithTag("SelectionBox").GetComponent<RectTransform>();
    }

#endregion

#region UPDATES

    private void Update()
    {
        HandleSelectionInputs();
        HandleMovementInputs();
        HandleUnitGhost();
    }

#endregion

#region External Called Logic

    public void SetLayerMaskInfo(Player player)
    {
        _unitsLayerMask = player.unitsLayerMask;
        _terrainLayerMask = player.terrainLayerMask;
        _woodsLayerMask = player.WoodsLayerMask;
        _interactableLayerMask = player.interactableLayerMask;
    }

#endregion

#region Intern Logic

    private void HandleSelectionInputs() => HandleMouseInputs();

    private void HandleMouseInputs()
    {
        MakeSelectionBox();
        MakeSelections();
    }

    private void HandleMovementInputs()
    {
        if (Input.GetKeyUp(KeyCode.Mouse1) && SelectionManager.Instance.SelectedUnits.Count > 0)
        {
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _interactableLayerMask))
            {
                // Check, if clicked on Unit
                if ((_unitsLayerMask.value & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    Unit unitToAttack = hit.transform.GetComponent<Unit>();
                
                    if (unitToAttack is not null)
                    {
                        foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                        {
                            // Continues for, if 'unitToAttack' cannot get attacked by his team member
                            if (unit.UnitData.Events.OnCheckForEnemyUnit?.Invoke(unitToAttack.UnitPlayerID) == true) continue;
                            
                            unit.CommandToAttack(unitToAttack);
                        }
                    }
                }
                // Check, if clicked on Terrain
                else if ((_terrainLayerMask.value & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    var selectedUnits = SelectionManager.Instance.SelectedUnits;
                    var center = CalculateCenterPoint(selectedUnits);
                    var formationPositions = CalculateFormationPositions(center, hit.point, selectedUnits, _unitSpacing);
                    var unitCount = 0;
                    
                    foreach (var unit in selectedUnits)
                    {
                        unit.RemoveTarget();
                        unit.CommandToDestination(formationPositions[unitCount]);
                        unitCount++;
                        //SpawnMoveToSprite(formationPositions[i]);
                    }
                }
                // Check, if clicked on Woods
                else if ((_woodsLayerMask.value & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    var selectedUnits = SelectionManager.Instance.SelectedUnits;
                    var center = CalculateCenterPoint(selectedUnits);
                    var formationPositions = CalculateFormationPositions(center, hit.point, selectedUnits, _unitSpacing);
                    var unitCount = 0;
                    
                    foreach (var unit in selectedUnits)
                    {
                        if (unit.UnitData.UnitType == UnitData.Type.Infantry)
                        {
                            unit.RemoveTarget();
                            unit.CommandToDestination(formationPositions[unitCount]);
                        }
                        unitCount++;
                    }
                }
                
                // Deselect Units
                if ( ! InputManager.Instance.Player.StickySelection)
                    SelectionManager.Instance.DeselectAll();
            }
        }
    }

    private void HandleUnitGhost()
    {
        if (Input.GetMouseButtonUp(0) && SelectionManager.Instance.SelectedUnits.Count > 0)
        {
            selectedUnitGameObject ??= Instantiate(SelectionManager.Instance.SelectedUnits.ToArray()[0].UnitData.UnitMesh);
            selectedUnitGameObject.transform.localScale *= 2f;
            
            var renderers = selectedUnitGameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer1 in renderers)
            {
                var mats = renderer1.materials;

                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = InputManager.Instance.Player.GhostMaterial;
                }

                renderer1.materials = mats;
            }

            var attackCircle = Instantiate(InputManager.Instance.Player.AttackCircle, selectedUnitGameObject.transform);
            var maxAttackRange = SelectionManager.Instance.SelectedUnits.ToArray()[0].UnitData.Events.OnGetMaxAttackRange.Invoke();
            var newCircleSize = maxAttackRange * 0.2f * Vector3.one / selectedUnitGameObject.transform.localScale.x;

            attackCircle.transform.position += Vector3.up;
            attackCircle.transform.localScale = newCircleSize;
        }
        
        else if (SelectionManager.Instance.SelectedUnits.Count > 0 && selectedUnitGameObject is not null)
            SetSelectionGhostPosition();
        
        else if (SelectionManager.Instance.SelectedUnits.Count == 0)
            if (selectedUnitGameObject is not null)
                DestroyUnitGhost();
    }

    private void SetSelectionGhostPosition()
    {
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _terrainLayerMask))
            selectedUnitGameObject.transform.position = hit.point;
    }

    private void DestroyUnitGhost()
    {
        Destroy(selectedUnitGameObject);
        selectedUnitGameObject = null;
    }

#endregion

#region Extracted Logic Methods

    private void MakeSelectionBox()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            selectionBox.gameObject.SetActive(true);
            startMousePosition = Input.mousePosition;
            mouseDownTime = Time.time;
            isDragging = false;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            
            if ((currentMousePosition - startMousePosition).magnitude > 5)
            {
                isDragging = true;
            }
        
            if (isDragging && mouseDownTime + dragDelay < Time.time)
            {
                ResizeSelectionBox();
            }
        }
    }

    private void MakeSelections()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            selectionBox.gameObject.SetActive(false);
            selectionBox.sizeDelta = Vector2.zero;

            if (isDragging)
            {
                SelectUnitsInBox();
            }
            else
            {
                // Handle simple clicking
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _unitsLayerMask))
                {
                    if (hit.collider.gameObject.TryGetComponent(out Unit unit))
                    {
                        // Return, if availableUnit is not a unit from player
                        if ( ! unit.CompareTag("Untagged")) return;
                        
                        HandleUnitSelection(unit);
                    }
                }
                else
                {
                    SelectionManager.Instance.DeselectAll();
                }
            }

            isDragging = false;
            mouseDownTime = 0;
        }
    }

    private void HandleUnitSelection(Unit unit)
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (SelectionManager.Instance.IsSelected(unit)) SelectionManager.Instance.Deselect(unit);
            else SelectionManager.Instance.Select(unit);
        }
        else
        {
            SelectionManager.Instance.DeselectAll();
            SelectionManager.Instance.Select(unit);
        }
    }

    private void SelectUnitsInBox()
    {
        // The rectangle is created from the current position and size of the selectionBox on the screen
        Rect selectionRect = new Rect(
            Mathf.Min(startMousePosition.x, Input.mousePosition.x),
            Mathf.Min(startMousePosition.y, Input.mousePosition.y),
            Mathf.Abs(startMousePosition.x - Input.mousePosition.x),
            Mathf.Abs(startMousePosition.y - Input.mousePosition.y));

        foreach (var unit in SelectionManager.Instance.AvailableUnits)
        {
            // Transforms the unit's world coordinates to screen coordinates
            Vector3 unitScreenPosition = camera.WorldToScreenPoint(unit.transform.position);

            // Ignore Z coordinate because ScreenPoint Z is always positive
            if (selectionRect.Contains(new Vector2(unitScreenPosition.x, unitScreenPosition.y)))
            {
                // Return, if availableUnit is not a unit from player
                if ( ! unit.CompareTag("Untagged")) return;
                
                SelectionManager.Instance.Select(unit);
            }
            else
            {
                SelectionManager.Instance.Deselect(unit);
            }
        }
    }

    private void ResizeSelectionBox()
    {
        float width = Input.mousePosition.x - startMousePosition.x;
        float height = Input.mousePosition.y - startMousePosition.y;

        selectionBox.anchoredPosition = startMousePosition + new Vector2(width / 2, height / 2);
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        Bounds bounds = new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);

        foreach (var availableUnit in SelectionManager.Instance.AvailableUnits)
        {
            // Return, if availableUnit is not a unit from player
            if ( ! availableUnit.CompareTag("Untagged")) return;
            
            if (UnitIsInSelectionBox(camera.WorldToScreenPoint(availableUnit.transform.position), bounds))
                SelectionManager.Instance.Select(availableUnit);
            else
                SelectionManager.Instance.Deselect(availableUnit);
        }
    }

#endregion

#region Extracted Return Methods

    private bool UnitIsInSelectionBox(Vector3 position, Bounds bounds)
    {
        return position.x > bounds.min.x
               && position.x < bounds.max.x
               && position.y > bounds.min.y
               && position.y < bounds.max.y;
    }
    
    private List<Vector3> CalculateFormationPositions(Vector3 center, Vector3 destination, HashSet<Unit> units, float spacing)
    {
        var positions = new List<Vector3>();
        
        var unitCount = units.Count;
        var width = Mathf.Min(_unitWidth, unitCount);
        var depth = Mathf.CeilToInt(unitCount / (float)_unitWidth);
        
        var offsetX = width / 2f - 0.5f;
        var offsetZ = depth / 2f - 0.5f;
        
        var direction = (destination - center).normalized;
        var rotation = Quaternion.LookRotation(direction);
        
        for (var i = 0; i < unitCount; i++)
        {
            var row = i / _unitWidth;
            var col = i % _unitWidth;
            
            var localPosition = new Vector3((col - offsetX) * spacing, 0, (row - offsetZ) * spacing);
            var rotatedPosition = rotation * localPosition;
            positions.Add(destination + rotatedPosition);
        }
        
        return positions;
    }
    
    private Vector3 CalculateCenterPoint(HashSet<Unit> units)
    {
        var center = Vector3.zero;
        foreach (var unit in units) center += unit.transform.position;
        center /= units.Count;
        return center;
    }

#endregion

    private void SpawnMoveToSprite(Vector3 destination)
    {
        GameObject newMoveToSprite = Instantiate(moveToSpritePrefab, destination, Quaternion.identity);
        newMoveToSprite.transform.position = destination + new Vector3(0f, 0.05f, 0f);
        Destroy(newMoveToSprite, 1f);
    }
}
