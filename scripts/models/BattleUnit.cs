using Godot;
using System;

public partial class BattleUnit : Resource
{
    [Export] public UnitStat unitStat;
    [Export] public Faction faction = Faction.Enemy;
    [Export] public Vector2I cellPos = Vector2I.Zero;
    [Export] public Direction direction = Direction.SE;
    [Export] public int currentHP = 1;

    public void SetData(UnitStat _unitState, Faction _faction, Vector2I _cellPos, Direction _direction, int _currentHP)
    {
        unitStat = _unitState;
        faction = _faction;
        cellPos = _cellPos;
        direction = _direction;
        currentHP = _currentHP;
    }
}
