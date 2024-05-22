using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("InGame-PlayerController")]
    [Space(10)]
    public LayerMask unitsLayerMask;
    public LayerMask terrainLayerMask;
    public LayerMask WoodsLayerMask;
    public LayerMask interactableLayerMask;
    public LayerMask RaycastLayerMask;

    public Color PlayerColor = Color.blue;
    public Color AllyColor = Color.green;
    public Color EnemyColor = Color.red;

    // ReSharper disable once NotAccessedField.Local
    [SerializeField] private int PlayerID;

    private void Awake() => PlayerID = GetInstanceID();
}
