using UnityEngine;

public class HashObject : MonoBehaviour
{
    private Vector3 lastPosition;

    // Debugging Fields:
    [SerializeField] private Vector2Int _currentHashKey;
    [SerializeField] private Vector2Int _lastHashKey;

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        InitializeHashKey();
    }
    
    private void InitializeHashKey()
    {
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(gameObject, _currentHashKey);
        _lastHashKey = _currentHashKey;
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
        /*
        if (Vector3.Distance(lastPosition, transform.position) > SpatialHash.CellSize * 0.5f)
        {
            SpatialHashManager.Instance.SpatialHash.RemoveObject(gameObject, lastPosition);
            SpatialHashManager.Instance.SpatialHash.AddObject(gameObject);
            lastPosition = transform.position;
            
            // DEBUGGING
            _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(lastPosition);
        }*/
        
        // Updates the Hash when Unit moves from one into another.
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        
        if (_currentHashKey == _lastHashKey) return;
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(gameObject, _lastHashKey);
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(gameObject, _currentHashKey);
        
        _lastHashKey = _currentHashKey;
    }

    private void OnDestroy()
    {
        SpatialHashManager.Instance.SpatialHash.RemoveObject(gameObject, transform.position);
    }
}
