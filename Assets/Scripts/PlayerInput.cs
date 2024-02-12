using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private new Camera camera;
    
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private LayerMask unitsLayerMask;
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float dragDelay = 0.1f;

    private float mouseDownTime;
    private Vector2 startMousePosition;
    
    public GameObject moveToSpritePrefab;
    
    private KeyCode lastHitKey;
    private KeyCode currentHitKey;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        HandleSelectionInputs();
        HandleMovementInputs();
    }

    private void HandleSelectionInputs()
    {
        HandleMouseInputs();
    }

    private void HandleMouseInputs()
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
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(false);

            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, unitsLayerMask)
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

    private void HandleMovementInputs()
    {
        if (Input.GetKeyUp(KeyCode.Mouse1) && SelectionManager.Instance.SelectedUnits.Count > 0)
        {
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, terrainLayerMask))
            {
                foreach (Unit unit in SelectionManager.Instance.SelectedUnits)
                {
                    unit.MoveToDestination(hit.point);
                    //SpawnMoveToSprite(hit.point);
                }
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
            if (UnitIsInSelectionBox(camera.WorldToScreenPoint(availableUnit.transform.position), bounds))
                SelectionManager.Instance.Select(availableUnit);
            else
                SelectionManager.Instance.Deselect(availableUnit);
        }
    }

    private bool UnitIsInSelectionBox(Vector3 position, Bounds bounds)
    {
        return position.x > bounds.min.x
               && position.x < bounds.max.x
               && position.y > bounds.min.y
               && position.y < bounds.max.y;
    }

    private void SpawnMoveToSprite(Vector3 destination)
    {
        GameObject newMoveToSprite = Instantiate(moveToSpritePrefab, destination, Quaternion.identity);
        newMoveToSprite.transform.position = destination + new Vector3(0f, 0.05f, 0f);
        Destroy(newMoveToSprite, 1f);
    }
}
