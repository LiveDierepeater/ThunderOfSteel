using System.Collections.Generic;
using UnityEngine;

public class SpatialHash
{
    private readonly Dictionary<Vector2Int, List<GameObject>> _grid = new();
    public const float CellSize = 500f;

    // Calculates the Hash-Key based of the position
    public Vector2Int CalculateHashKey(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.z / CellSize));
    }

    // Adds an Object to the Hash
    public void AddObject(GameObject obj)
    {
        Vector2Int hashKey = CalculateHashKey(obj.transform.position);
        if (!_grid.ContainsKey(hashKey))
        {
            _grid[hashKey] = new List<GameObject>();
        }
        _grid[hashKey].Add(obj);
    }

    // Removes an Object from the Hash
    public void RemoveObject(GameObject obj, Vector3 oldPosition)
    {
        Vector2Int hashKey = CalculateHashKey(oldPosition);
        if (_grid.ContainsKey(hashKey))
        {
            _grid[hashKey].Remove(obj);
            if (_grid[hashKey].Count == 0)
            {
                _grid.Remove(hashKey);
            }
        }
    }

    // Find all Objects close to the 'position'
    public List<GameObject> GetNearbyObjects(Vector3 position)
    {
        Vector2Int hashKey = CalculateHashKey(position);
        if (_grid.TryGetValue(hashKey, out var nearbyObjects))
        {
            return nearbyObjects;
        }
        return new List<GameObject>();
    }
}
