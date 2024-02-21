using System;

public struct UnitEvents
{
    //public delegate void MoveUnitInRange();
    public Action OnMoveUnitInRange;

    //public delegate void UnitIsInRange();
    public Action OnUnitIsInRange;
}
