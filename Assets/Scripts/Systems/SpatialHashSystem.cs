using System;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash
{
    public int RegisteredUnits { get; private set; }
    private List<Unit> nearbyUnits = new();
    
    private readonly Dictionary<Vector2Int, List<Unit>> _grid = new();
    private const float CellSize = 500f;
    
    /// <summary>
    /// Is used for Grabbing nearby enemy Units.
    /// Consider all nearby Grids, the current Grid inclusive.
    /// </summary>
    private static readonly List<Vector2> offsets = new List<Vector2>
    {
        new Vector2(0, 0), // current Grid
        new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1),
        new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1),
        new Vector2(-1, -1), new Vector2(-1, 0)
    };

    private List<Vector2> precomputedOffsets;
    
    // Calculates the Hash-Key based of the position
    public Vector2Int CalculateHashKey(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.z / CellSize));
    }
    
    // Adds an Object to the Hash
    public void AddObject(Unit obj)
    {
        Vector2Int hashKey = CalculateHashKey(obj.transform.position);
        AddObjectWithHashKey(obj, hashKey);
    }
    
    public void AddObjectWithHashKey(Unit obj, Vector2Int hashKey)
    {
        if (!_grid.ContainsKey(hashKey))
        {
            _grid[hashKey] = new List<Unit>();
        }

        if (_grid[hashKey].Contains(obj))
        {
            throw new Exception("Already Exists");
        }
        _grid[hashKey].Add(obj);
        RegisteredUnits += 1;
    }

    // Removes an Object from the Hash
    public void RemoveObject(Unit obj, Vector3 oldPosition)
    {
        Vector2Int hashKey = CalculateHashKey(oldPosition);
        RemoveObjectWithHashKey(obj, hashKey);
    }

    public void RemoveObjectWithHashKey(Unit obj, Vector2Int hashKey)
    {
        if (_grid.ContainsKey(hashKey))
        {
            _grid[hashKey].Remove(obj);
            if (_grid[hashKey].Count == 0)
            {
                _grid.Remove(hashKey);
            }
        }
        RegisteredUnits -= 1;
    }

    // Find all Units close to the 'position'
    public List<Unit> GetNearbyUnits(Vector3 position, bool alsoSearchInNearbyHashKeys)
    {
        Vector2Int hashKey = CalculateHashKey(position);
        if (_grid.TryGetValue(hashKey, out var nearbyUnits))
        {
            if (nearbyUnits.Count == 0)
            {
                GetNearbyUnitsInNearbyHashKeys(position);
            }
            else
                return nearbyUnits;
        }
        return new List<Unit>();
    }
    
    public List<Unit> GetNearbyUnitsInNearbyHashKeys(Vector3 position)
    {
        nearbyUnits.Clear();
        var position2D = new Vector2(position.x, position.z);
        
        // Calculate the main-HashKey for the current position
        //var mainKey = CalculateHashKey(position);
        
        foreach (var offset in precomputedOffsets)
        {
            // Jump first/own HashKey
            //if (offset == Vector2Int.zero) continue;
            
            // Calculate the HashKey for the current nearby Grid
            var key = CalculateHashKey(position2D + offset);
            
            // If the current grid contains units, add them to the 'nearbyUnits' list
            if (_grid.TryGetValue(key, out var unitsInCurrentGrid))
            {
                nearbyUnits.AddRange(unitsInCurrentGrid);
            }
        }
        
        return nearbyUnits;
    }

    public void InitializeCellOffsetsForDistanceCalculation()
    {
        precomputedOffsets = new List<Vector2>();
        foreach (var offset in offsets)
        {
            precomputedOffsets.Add(new Vector2(offset.x * CellSize, offset.y * CellSize));
        }
    }
}
