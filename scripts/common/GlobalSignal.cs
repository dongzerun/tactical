using Godot;
using System;

public partial class GlobalSignal:Node
{
    public static event Action<Unit> UnitDiedEvent;
    public static event Action<Vector2,int> ShowFloatingTextEvent;
    public static event Action<Vector2,int> ShowFloatingHealTextEvent;
    
    public static void RaiseUnitDied(Unit unit)
    {
        UnitDiedEvent?.Invoke(unit);
    }

    public static void RaiseShowFloatingTextEvent(Vector2 pos, int amount)
    {
        ShowFloatingTextEvent?.Invoke(pos, amount);
    }
    
    public static void RaiseShowFloatingHealTextEvent(Vector2 pos, int amount)
    {
        ShowFloatingHealTextEvent?.Invoke(pos, amount);
    }
}
