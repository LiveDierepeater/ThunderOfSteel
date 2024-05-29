using System;
using UnityEngine;

public class UVisibilityManager : UnitSystem
{
    [Header("Data")]
    public FactionData FactionData;
    
    [HideInInspector] public Texture2D MaskTexture;
    [HideInInspector] public Color PlayerColor;
    
    [Header("References")]
    [SerializeField] private Transform _mesh;
    [SerializeField] private Transform _meshSlot;
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    
    private float _currentCameraZoomLevel;
    private float _standardScale;
    private float _bigScale;
    private float _currentScale;
    private bool _isCameraZoomedOut = true;
    private bool _isMeshOnChip = true;
    
    private static readonly int PlayerColor1 = Shader.PropertyToID("_PlayerColor");
    private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

    protected override void Awake()
    {
        base.Awake();
        Unit.OnInitializeChip += InitializeChip;
        Unit.OnUnitDeath += HandleUnitDeath;
    }

    private void InitializeChip()
    {
        _propertyBlock ??= new MaterialPropertyBlock();

        MaskTexture = FactionData.FactionChipMasks[(int)Unit.UnitData.Faction];
        PlayerColor = Unit.PlayerColor;

        // Set the initial values
        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetTexture(MaskTex, MaskTexture);
        _propertyBlock.SetColor(PlayerColor1, PlayerColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void Start()
    {
        _standardScale = _renderer.transform.localScale.x;
        _bigScale = _standardScale * 1.3f;
        _currentScale = _bigScale;
        InputManager.Instance.OnCameraUpdate += HandleCameraUpdate;
        
        InitializeChipScale();
    }

    private void InitializeChipScale() => transform.root.localScale *= _standardScale;

    private void HandleUnitDeath()
    {
        Unit.OnInitializeChip -= InitializeChip;
        InputManager.Instance.OnCameraUpdate -= HandleCameraUpdate;
        Unit.OnUnitDeath -= HandleUnitDeath;
    }

    private void HandleCameraUpdate(float currentZoomLevel)
    {
        _currentCameraZoomLevel = currentZoomLevel;
        
        if (IsCameraZoomedOut() != _isCameraZoomedOut)
        {
            SwitchChipOnOff();
            SwitchMeshLocation();
            SwitchCurrentScale();
            SwitchCameraZoomedOutBool();
        }
        
        ScaleUnitAlongWithCameraZoomLevel(currentZoomLevel);
        CheckMeshVisibility();
        UpdateChipState();
    }

    private bool IsCameraZoomedOut() => _currentCameraZoomLevel > 0.25f;

    private void SwitchChipOnOff() => _renderer.enabled = ! _renderer.enabled;

    private void SwitchCameraZoomedOutBool() => _isCameraZoomedOut = !_isCameraZoomedOut;

    private void SwitchMeshLocation()
    {
        if ( ! _isMeshOnChip)
        {
            _mesh.localPosition = _meshSlot.localPosition;
            _isMeshOnChip = !_isMeshOnChip;
        }
        else
        {
            _mesh.localPosition = Vector3.zero;
            _isMeshOnChip = !_isMeshOnChip;
        }
    }

    private void SwitchCurrentScale()
    {
        if (Math.Abs(_currentScale - _bigScale) < 0.1f)
            _currentScale = _standardScale;
        else if (Math.Abs(_currentScale - _standardScale) < 0.1f)
            _currentScale = _bigScale;
    }

    private void ScaleUnitAlongWithCameraZoomLevel(float currentZoomLevel)
    {
        //var cZL = (currentZoomLevel + 1f) * 0.65f;
        var newScale = Mathf.Clamp(currentZoomLevel * 2f, 0.2f, 3f)  * _currentScale;
        transform.root.localScale = new Vector3(newScale, newScale, newScale);
    }

    private void CheckMeshVisibility()
    {
        if ( ! transform.root.CompareTag("AI")) return;
        
        if (Unit.IsSpotted)
        {
            if (_mesh.gameObject.activeSelf) return;
            
            _mesh.gameObject.SetActive(true);
        }
        else
        {
            if ( ! _mesh.gameObject.activeSelf) return;
                
            _mesh.gameObject.SetActive(false);
        }
    }

    private void UpdateChipState()
    {
        if (!transform.root.CompareTag("AI")) return;

        if (Unit.IsSpotted)
        {
            if (IsCameraZoomedOut())
            {
                _renderer.enabled = true;
                _mesh.localPosition = _meshSlot.localPosition;
                _isMeshOnChip = true;
            }
            else
            {
                _renderer.enabled = false;
                _mesh.localPosition = Vector3.zero;
                _isMeshOnChip = false;
            }
        }
        else
        {
            _renderer.enabled = true;
            _mesh.localPosition = _meshSlot.localPosition;
            _isMeshOnChip = true;
        }
    }
}
