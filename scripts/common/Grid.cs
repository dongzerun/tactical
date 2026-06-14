using System.Collections.Generic;
using Godot;

public class ReachableCellsInfo
{
    public Dictionary<Vector2I,int> CostSofar;
    public Dictionary<Vector2I, Vector2I> Parents;
}