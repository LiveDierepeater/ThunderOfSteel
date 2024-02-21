using UnityEngine;

public class HashObject : MonoBehaviour
{
    private Vector3 lastPosition;

    // Debugging Fields:
    [SerializeField] private Vector2 _currentHashKey;

    private void OnEnable()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        SpatialHashManager.Instance.SpatialHash.AddObject(gameObject);
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        SpatialHashManager.Instance.SpatialHash.RemoveObject(gameObject, lastPosition);
    }

    private void HandleTick()
    {
        OnUnitMoveHashUpdate();
    }
    
    private void OnUnitMoveHashUpdate()
    {
        // Updates the Hash when Unit moves from one into another.
        
        if (Vector3.Distance(lastPosition, transform.position) > SpatialHash.CellSize * 0.5f)
        {
            SpatialHashManager.Instance.SpatialHash.RemoveObject(gameObject, lastPosition);
            SpatialHashManager.Instance.SpatialHash.AddObject(gameObject);
            lastPosition = transform.position;
            
            // DEBUGGING
            _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(lastPosition);
        }
    }

    private void OnDestroy()
    {
        SpatialHashManager.Instance.SpatialHash.RemoveObject(gameObject, transform.position);
    }
}
