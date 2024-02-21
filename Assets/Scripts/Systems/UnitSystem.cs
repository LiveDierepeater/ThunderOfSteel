using UnityEngine;

public abstract class UnitSystem : MonoBehaviour
{
    protected Unit Unit;

    protected virtual void Awake()
    {
        Unit = transform.root.GetComponent<Unit>();
    }
}
