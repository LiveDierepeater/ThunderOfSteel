using System;
using UnityEngine;

public class UVisibilityManager : UnitSystem
{
    public FactionData FactionData;
    
    public Texture2D MaskTexture;
    public Color PlayerColor;

    [SerializeField] private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

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
        _propertyBlock.SetTexture("_MaskTex", MaskTexture);
        _propertyBlock.SetColor("_PlayerColor", PlayerColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void OnDestroy() => Unit.OnInitializeChip -= InitializeChip;

    // private void Update()
    // {
    //     // Update the playerColor at runtime
    //     _renderer.GetPropertyBlock(_propertyBlock);
    //     _propertyBlock.SetColor("_PlayerColor", PlayerColor);
    //     _renderer.SetPropertyBlock(_propertyBlock);
    // }
}
