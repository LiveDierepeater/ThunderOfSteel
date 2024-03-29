using UnityEngine;

public class InGamePlayerController : MonoBehaviour
{
    public GameObject moveToSpritePrefab;
    
    private new Camera camera;
    
    [SerializeField] private RectTransform selectionBox;
    private LayerMask _unitsLayerMask;
    private LayerMask _terrainLayerMask;
    private LayerMask _interactableLayerMask;
    [SerializeField] private float dragDelay = 0.1f;

    #region Internal Fields

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
    }

    #endregion

    #region External Called Logic

    public void SetLayerMaskInfo(Player player)
    {
        _unitsLayerMask = player.unitsLayerMask;
        _terrainLayerMask = player.terrainLayerMask;
        _interactableLayerMask = player.interactableLayerMask;
    }

    #endregion

    #region Intern Logic

    private void HandleSelectionInputs()
    {
        HandleMouseInputs();
    }

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
                            unit.CommandToAttack(unitToAttack);
                        }
                    }
                }
                // Check, if clicked on Terrain
                else if ((_terrainLayerMask.value & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                    {
                        unit.RemoveTarget();
                        unit.CommandToDestination(hit.point);
                        //SpawnMoveToSprite(hit.point);
                    }
                }
            }
        }
    }

    #endregion

    #region Extracted Logic Methods

    private void MakeSelectionBox()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startMousePosition = Input.mousePosition;
            mouseDownTime = Time.time;
        }
        else if (Input.GetKey(KeyCode.Mouse0) && mouseDownTime + dragDelay < Time.time)
        {
            ResizeSelectionBox();
        }
    }

    private void MakeSelections()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(false);

            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _unitsLayerMask)
                && hit.collider.gameObject.TryGetComponent(out Unit unit))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (SelectionManager.Instance.IsSelected(unit))
                    {
                        SelectionManager.Instance.Deselect(unit);
                    }
                    else
                    {
                        SelectionManager.Instance.Select(unit);
                    }
                }
                else
                {
                    SelectionManager.Instance.DeselectAll();
                    SelectionManager.Instance.Select(unit);
                }
            }
            else if (mouseDownTime + dragDelay > Time.time)
            {
                SelectionManager.Instance.DeselectAll();
            }
            
            mouseDownTime = 0;
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

    #endregion

    private void SpawnMoveToSprite(Vector3 destination)
    {
        GameObject newMoveToSprite = Instantiate(moveToSpritePrefab, destination, Quaternion.identity);
        newMoveToSprite.transform.position = destination + new Vector3(0f, 0.05f, 0f);
        Destroy(newMoveToSprite, 1f);
    }
}
