using Cinemachine;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    
    [Space(10)]
    
    public float MoveSpeed = 100f;
    public float RotateSpeed = 100f;
    public float DragPanSpeed = 0.5f;
    
    [Space(10)]
    
    public bool UseEdgeScrolling;
    public int EdgeScrollSize = 20;

    [Space(10)]
    
    public bool UseDragPanMove;
    public bool UseMouseCameraRotation;

    [Space(10)]
    
    public float ZoomSpeed = 3f;
    public float ZoomAmount = 30f;
    public float FollowOffsetMin = 30f;
    public float FollowOffsetMax = 400f;

    public float CurrentZoomLevel { get; private set; }

    // Private Fields
    
    private float _standardMoveSpeed;
    private float _standardDragPanSpeed;
    private float _standardRotateSpeed;
    //private float _standardZoomAmount;
    private float _fastMoveSpeed;
    private float _fastDragPanSpeed;
    private float _fastRotateSpeed;
    private float _fastZoomAmount;
    
    private bool _isMouseRotatingActive;
    private bool _isDragPanMoveActive;
    private Vector2 _lastMousePosition;
    private Vector3 _followOffset;

    private void Awake()
    {
        _followOffset = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        _standardMoveSpeed = MoveSpeed;
        _standardRotateSpeed = RotateSpeed;
        _standardDragPanSpeed = DragPanSpeed * 0.3f;
        //_standardZoomAmount = ZoomAmount * 0.2f;
        _fastMoveSpeed = _standardMoveSpeed * 3f;
        _fastRotateSpeed = _standardRotateSpeed * 1.5f;
        _fastDragPanSpeed = _standardDragPanSpeed;
        _fastZoomAmount = ZoomAmount;
    }

    private void Update()
    {
        ModifyCameraSpeedWithZoomLevel();
        
        HandleKeyboardCameraMovement();
        if (UseEdgeScrolling) HandleEdgeScrollingCameraMovement();
        if (UseDragPanMove) HandleDragPanCameraMovement();
        
        HandleKeyboardCameraRotation();
        if (UseMouseCameraRotation) HandleMouseCameraRotation();

        HandleCameraZoom_MoveForward();
    }

    private void HandleKeyboardCameraMovement()
    {
        Vector3 inputDirection = new Vector3(0, 0, 0);
        
        if (Input.GetKey(KeyCode.W)) inputDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDirection.x = +1f;
        
        CalculateCameraMovement(inputDirection);
    }

    private void HandleKeyboardCameraRotation()
    {
        float rotateDirection = 0f;
        
        if (Input.GetKey(KeyCode.Q)) rotateDirection = +1f;
        if (Input.GetKey(KeyCode.E)) rotateDirection = -1f;
        
        CalculateCameraRotation(rotateDirection);
    }

    private void HandleEdgeScrollingCameraMovement()
    {
        Vector3 inputDirection = new Vector3(0, 0, 0);
        
        if (Input.mousePosition.x < EdgeScrollSize) inputDirection.x = -0.5f;
        if (Input.mousePosition.y < EdgeScrollSize) inputDirection.z = -0.5f;
        if (Input.mousePosition.x > Screen.width - EdgeScrollSize) inputDirection.x = +0.5f;
        if (Input.mousePosition.y > Screen.height - EdgeScrollSize) inputDirection.z = +0.5f;
        
        CalculateCameraMovement(inputDirection);
    }

    private void HandleDragPanCameraMovement()
    {
        Vector3 inputDirection = new Vector3(0, 0, 0);
        
        if (Input.GetMouseButtonDown(1))
        {
            _isDragPanMoveActive = true;
            _lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            _isDragPanMoveActive = false;
        }

        if (_isDragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - _lastMousePosition;
                
            inputDirection.x = -mouseMovementDelta.x * DragPanSpeed;
            inputDirection.z = -mouseMovementDelta.y * DragPanSpeed;
                
            _lastMousePosition = Input.mousePosition;
        }

        CalculateCameraMovement(inputDirection);
    }

    private void HandleMouseCameraRotation()
    {
        float rotateDirection = 0f;
        
        Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - _lastMousePosition;
        
        if (Input.GetMouseButtonDown(2))
        {
            _isMouseRotatingActive = true;
            _lastMousePosition = Input.mousePosition;
        }
        
        if (Input.GetMouseButtonUp(2))
        {
            _isMouseRotatingActive = false;
        }
        
        if (_isMouseRotatingActive)
        {
            if (mouseMovementDelta.x > 0)
            {
                rotateDirection = mouseMovementDelta.x * 0.003f;
            }
            else if (mouseMovementDelta.x < 0)
            {
                rotateDirection = mouseMovementDelta.x * 0.003f;
            }
        }
        
        CalculateCameraRotation(rotateDirection);
    }

    private void HandleCameraZoom_MoveForward()
    {
        Vector3 zoomDirection = _followOffset.normalized;
        
        if (Input.mouseScrollDelta.y > 0) _followOffset -= zoomDirection * ZoomAmount;
        if (Input.mouseScrollDelta.y < 0) _followOffset += zoomDirection * ZoomAmount;
        
        if (_followOffset.magnitude < FollowOffsetMin) _followOffset = zoomDirection * FollowOffsetMin;
        if (_followOffset.magnitude > FollowOffsetMax) _followOffset = zoomDirection * FollowOffsetMax;
        
        _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(_cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, _followOffset, ZoomSpeed * Time.deltaTime);
    }

    private void ModifyCameraSpeedWithZoomLevel()
    {
        var angledAmount = (_cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.magnitude / FollowOffsetMax);
        var newYFollowOffset = Mathf.Clamp(-_followOffset.z * Mathf.Clamp01(angledAmount * 5f) - (40f * angledAmount), 20f, 1000f);
        
        _followOffset = new Vector3(_followOffset.x, newYFollowOffset, _followOffset.z);
        
        MoveSpeed = _fastMoveSpeed * Mathf.Clamp01(angledAmount * 2f);
        RotateSpeed = _fastRotateSpeed * Mathf.Clamp01(angledAmount * 7.5f);
        DragPanSpeed = _fastDragPanSpeed * Mathf.Clamp01(angledAmount * 10f);
        ZoomAmount = _fastZoomAmount * angledAmount * 2f;

        CurrentZoomLevel = angledAmount;
        InputManager.Instance.OnCameraUpdate?.Invoke(CurrentZoomLevel);
    }

    private void CalculateCameraMovement(Vector3 inputDirection)
    {
        Vector3 moveDirection = transform.forward * inputDirection.z + transform.right * inputDirection.x;
        transform.position += moveDirection * (MoveSpeed * Time.deltaTime);
    }

    private void CalculateCameraRotation(float rotateDirection) => transform.eulerAngles += new Vector3(0, rotateDirection * RotateSpeed * Time.deltaTime, 0);

    //private bool IsCameraZoomedOut() => _followOffset.magnitude > ZoomedBarrier;
}
