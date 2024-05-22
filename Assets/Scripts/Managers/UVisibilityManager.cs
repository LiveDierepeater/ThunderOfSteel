using System;
using UnityEngine;

public class UVisibilityManager : UnitSystem
{
    public FactionData FactionData;
    
    public Texture2D MaskTexture;
    public Color PlayerColor;

    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    private CameraSystem _cameraSystem;
    private float _cameraZoomAmount;
    private float _standardScale = 5f;
    
    private static readonly int PlayerColor1 = Shader.PropertyToID("_PlayerColor");
    private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

    protected override void Awake()
    {
        base.Awake();
        Unit.OnInitializeChip += InitializeChip;
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
        if (Unit.UnitData.Chip == UnitData.ChipType.Big) _standardScale *= 2f;
        Invoke(nameof(InitializeCameraSystemReference), 0.2f);
    }

    private void InitializeCameraSystemReference() => _cameraSystem = InputManager.Instance.CameraSystem;

    private void OnDestroy() => Unit.OnInitializeChip -= InitializeChip;

    private void Update()
    {
        _cameraZoomAmount = _cameraSystem.CurrentZoomLevel;
        
        // TODO: Implement automatic-scaling of units
    }
}
