using System.Collections.Generic;

public class UnitManager
{
    private static UnitManager _instance;

    public static UnitManager Instance => _instance ??= new UnitManager();

    // Dictionary for mapping PlayerIDs to the lists of units per team
    private readonly Dictionary<int, List<Unit>> _teamUnits = new Dictionary<int, List<Unit>>();

    // List of existing PlayerIDs
    private readonly List<int> _playerIDs = new List<int>();

    public const int AI_ID = 10101;
    public const int ALLY_ID = 10102;

    public void AddUnit(Unit unit, int unitsPlayerID)
    {
        if (unit.CompareTag("AI"))
        {
            unit.UnitPlayerID = AI_ID;
            unitsPlayerID = AI_ID;
        }

        if (unit.CompareTag("Ally"))
        {
            unit.UnitPlayerID = ALLY_ID;
            unitsPlayerID = ALLY_ID;
        }
        
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

        //Debug.Log(GetUnitsForPlayerID(unitsPlayerID).Count.ToString());
    }

    /// <summary>
    /// Method to get the unit list for a specific PlayerID.
    /// If the PlayerID does not exist, return an empty list.
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public List<Unit> GetUnitsForPlayerID(int playerID) => _teamUnits.TryGetValue(playerID, out var id) ? id : new List<Unit>();
}
