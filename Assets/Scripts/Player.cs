using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("InGame-PlayerController")]
    [Space(10)]
    public LayerMask unitsLayerMask;
    public LayerMask terrainLayerMask;
    public LayerMask interactableLayerMask;
}
