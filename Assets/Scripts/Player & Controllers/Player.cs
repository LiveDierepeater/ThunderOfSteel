using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Physics Layers")]
    public LayerMask unitsLayerMask;
    public LayerMask terrainLayerMask;
    public LayerMask WoodsLayerMask;
    public LayerMask interactableLayerMask;
    public LayerMask RaycastLayerMask;

    [Space(10)]
    [Header("UI Player Colors")]
    
    public Color PlayerColor = Color.blue;
    public Color AllyColor = Color.green;
    public Color EnemyColor = Color.red;

    public GameObject AttackCircle;
    public Material GhostMaterial;

    // ReSharper disable once NotAccessedField.Local
    [SerializeField] private int PlayerID;

    [Header("Settings")] public bool StickySelection;

    private void Awake() => PlayerID = GetInstanceID();
}
