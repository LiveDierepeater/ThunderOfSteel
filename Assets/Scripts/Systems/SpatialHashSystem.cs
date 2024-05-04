using System.Collections.Generic;
using UnityEngine;

public class SpatialHash
{
    public int RegisteredUnits { get; private set; }

    private readonly Dictionary<Vector2Int, Dictionary<int, List<Unit>>> _grid = new Dictionary<Vector2Int, Dictionary<int, List<Unit>>>();
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
    
    // Calculates the Hash-Key based of the position
    public Vector2Int CalculateHashKey(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.z / CellSize));
    }
    
    public void AddObjectWithHashKey(Unit unit, Vector2Int hashKey)
    {
        int playerID = unit.UnitPlayerID;

        if ( ! _grid.ContainsKey(hashKey))
        {
            _grid[hashKey] = new Dictionary<int, List<Unit>>();
        }

        if ( ! _grid[hashKey].ContainsKey(playerID))
        {
            _grid[hashKey][playerID] = new List<Unit>();
        }

        _grid[hashKey][playerID].Add(unit);
        RegisteredUnits += 1;
    }
    
    public void RemoveObjectWithHashKey(Unit unit, Vector2Int hashKey)
    {
        int playerID = unit.UnitPlayerID;

        if (_grid.ContainsKey(hashKey) && _grid[hashKey].ContainsKey(playerID))
        {
            _grid[hashKey][playerID].Remove(unit);

            if (_grid[hashKey][playerID].Count == 0)
            {
                _grid[hashKey].Remove(playerID);
            }

            if (_grid[hashKey].Count == 0)
            {
                _grid.Remove(hashKey);
            }
        }
        RegisteredUnits -= 1;
    }

    // Find all Units close to the 'position' from different teams
    public List<Unit> GetNearbyUnitsFromDifferentTeams(Vector3 position, int playerID)
    {
        var nearbyUnits = new List<Unit>();
        
        // Calculate the hash key for the current position
        Vector2Int mainHashKey = CalculateHashKey(position);
        
        // Add units from the main hash key (current position)
        AddUnitsFromHashKey(mainHashKey, playerID, nearbyUnits);
        
        // Iterate through neighboring hash keys
        for (int i = 0; i < 8; i++)
        {
            Vector2Int neighborHashKey = new Vector2Int(mainHashKey.x + (int)offsets[i].x, mainHashKey.y + (int)offsets[i].y);
            
            // Add units from the neighboring hash key
            AddUnitsFromHashKey(neighborHashKey, playerID, nearbyUnits);
        }
        
        return nearbyUnits;
    }
    
    // Helper method to add units from a specific hash key if they belong to a different team
    private void AddUnitsFromHashKey(Vector2Int hashKey, int playerID, List<Unit> nearbyUnits)
    {
        if (_grid.TryGetValue(hashKey, out var teams))
        {
            foreach (var kvp in teams)
            {
                // Continue FOREACH, if the current 'kvp.Key' is the allied Units-Dictionary
                if (kvp.Key == playerID) continue;
                
                nearbyUnits.AddRange(kvp.Value);
            }
        }
    }
}
