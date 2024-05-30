using UnityEngine;

public class HashObject : UnitSystem
{
    private Vector3 lastPosition;

    // Debugging Fields:
    [SerializeField] private Vector2Int _currentHashKey;
    [SerializeField] private Vector2Int _lastHashKey;

    private void Start()
    {
        TickManager.Instance.TickSystem.OnTick += HandleTick;
        Unit.Events.OnUnitDeath += OnDisable;
        InitializeHashKey();
    }
    
    private void InitializeHashKey()
    {
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(Unit, _currentHashKey);
        _lastHashKey = _currentHashKey;
    }

    private void OnDisable()
    {
        TickManager.Instance.TickSystem.OnTick -= HandleTick;
        Unit.Events.OnUnitDeath -= OnDisable;
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(Unit, _lastHashKey);
    }

    private void HandleTick()
    {
        OnUnitMoveHashUpdate();
    }
    
    private void OnUnitMoveHashUpdate()
    {
        // Updates the Hash when Unit moves from one into another.
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        
        if (_currentHashKey == _lastHashKey) return;
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(Unit, _lastHashKey);
        SpatialHashManager.Instance.SpatialHash.AddObjectWithHashKey(Unit, _currentHashKey);
        
        _lastHashKey = _currentHashKey;
    }

    private void OnDestroy()
    {
        _currentHashKey = SpatialHashManager.Instance.SpatialHash.CalculateHashKey(transform.position);
        SpatialHashManager.Instance.SpatialHash.RemoveObjectWithHashKey(Unit, _lastHashKey);
    }
}
