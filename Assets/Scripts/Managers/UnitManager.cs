using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private static UnitManager Instance { get; set; }

    // Dictionary for mapping PlayerIDs to the lists of units per team
    private readonly Dictionary<int, List<Unit>> _teamUnits = new Dictionary<int, List<Unit>>();

    // List of existing PlayerIDs
    private readonly List<int> _playerIDs = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddUnit(Unit unit, int unitsPlayerID)
    {
        // If there are no PlayerIDs, add the first one
        if (_playerIDs.Count == 0)
        {
            _playerIDs.Add(unitsPlayerID);
            
            // Create a new first team List
            List<Unit> newTeamList = new List<Unit> { unit };

            // Add the new list to the dictionary
            _teamUnits.Add(unitsPlayerID, newTeamList);
        }
        else
        {
            // Check if the PlayerID already exists
            // If so, add the unit to the appropriate list
            if (_playerIDs.Contains(unitsPlayerID))
                _teamUnits[unitsPlayerID].Add(unit);
            else
            {
                // If not, add the PlayerID to the list
                _playerIDs.Add(unitsPlayerID);
                
                // Create a new list for the new team
                List<Unit> newTeamList = new List<Unit> { unit };

                // Add the new list to the dictionary
                _teamUnits.Add(unitsPlayerID, newTeamList);
            }
        }
    }

    public void RemoveUnit(Unit unit, int unitsPlayerID)
    {
        // Returns, if unitsPlayerID is not registered -> so this unit should not have been registered before
        if ( ! _playerIDs.Contains(unitsPlayerID)) return;
        
        // Remove unit from it's _playerID's List
        _teamUnits[unitsPlayerID].Remove(unit);
        
        // Remove unitsPlayerID from _teamUnits List, if there are no more units registered under this unitsPlayerID
        if (_teamUnits[unitsPlayerID].Count == 0) _teamUnits.Remove(unitsPlayerID);
    }

    // Method to get the unit list for a specific PlayerID
    public List<Unit> GetUnitsForPlayerID(int playerID)
    {
        if (_teamUnits.TryGetValue(playerID, out var id))
            return id;
        
        // If the PlayerID does not exist, return an empty list
        return new List<Unit>();
    }
    
    // Method to get the combined unit lists for all enemy PlayerID's
    
}
