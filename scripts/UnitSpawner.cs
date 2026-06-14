using Godot;
using System;

public partial class UnitSpawner : Node
{
    [Export] private PackedScene _unitScene;
    [Export] private Node _container;
    [Export] private GameArea _gameArea;

    public static Vector2 DEFAULT_OFFSET = new Vector2(0.0f,-8.0f);
    public Unit spawnUnitWithPosition(Vector2 pos, UnitInfo unitInfo)
    {
        var unitInstance = _unitScene.Instantiate<Unit>();
        unitInstance.Position = pos;
        unitInstance.faction = unitInfo.faction;
        unitInstance.unitStat = unitInfo.UnitStat;
        if (_container != null)
        {
            _container.AddChild(unitInstance);
        }
        else
        {
            AddChild(unitInstance);
        }

        unitInstance.updateVisual();
        return unitInstance;
    }

    public Unit SpawnUnitInCell(Vector2I cell, UnitInfo unitInfo)
    {
        var worldPos = _gameArea.getGlobalFromTile(cell)+DEFAULT_OFFSET;
        return spawnUnitWithPosition(worldPos, unitInfo);
    }
}
