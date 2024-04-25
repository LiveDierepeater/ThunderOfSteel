using System.Collections.Generic;

public class SelectionManager
{
    private static SelectionManager _instance;

    public static SelectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SelectionManager();
            }

            return _instance;
        }
    }

    public readonly HashSet<Unit> SelectedUnits = new HashSet<Unit>();
    public readonly List<Unit> AvailableUnits = new List<Unit>();

    private SelectionManager() { }

#region Selected Units Logic

    public void Select(Unit unit)
    {
        unit.OnSelected();
        SelectedUnits.Add(unit);
    }

    public void Deselect(Unit unit)
    {
        unit.OnDeselected();
        SelectedUnits.Remove(unit);
    }

    public void DeselectAll()
    {
        foreach (Unit unit in SelectedUnits)
        {
            unit.OnDeselected();
        }
        
        SelectedUnits.Clear();
    }

    public bool IsSelected(Unit unit)
    {
        return SelectedUnits.Contains(unit);
    }

#endregion

#region Available Units Logic

    public void RemoveAvailableUnit(Unit unitToRemove)
    {
        AvailableUnits.Remove(unitToRemove);
    }

#endregion
}
