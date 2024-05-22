using UnityEngine;

public class UVisibilityManager : UnitSystem
{
    [Header("Data")]
    public FactionData FactionData;
    
    [HideInInspector] public Texture2D MaskTexture;
    [HideInInspector] public Color PlayerColor;
    
    [Header("References")]
    [SerializeField] private Transform _meshSlot;
    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    
    private float _currentCameraZoomLevel;
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
        InputManager.Instance.OnCameraUpdate += HandleCameraUpdate;
    }

    private void OnDestroy()
    {
        Unit.OnInitializeChip -= InitializeChip;
        InputManager.Instance.OnCameraUpdate -= HandleCameraUpdate;
    }

    private void HandleCameraUpdate(float currentZoomLevel)
    {
        _currentCameraZoomLevel = currentZoomLevel;
        
        // TODO: Implement automatic-scaling of units
    }
}
