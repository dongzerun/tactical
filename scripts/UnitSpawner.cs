using Godot;
using System;

public partial class UnitSpawner : Node
{
    [Export] private PackedScene unitScene;
    [Export] private Node container;
    [Export] private GameArea gameArea;

    public static Vector2 DEFAULT_OFFSET = new Vector2(0.0f,-8.0f);
    public Unit spawnUnitWithPosition(Vector2 pos, UnitInfo unitInfo)
    {
        var unitInstance = unitScene.Instantiate<Unit>();
        unitInstance.Position = pos;
        unitInstance.faction = unitInfo.faction;
        unitInstance.unitStat = unitInfo.UnitStat;
        if (container != null)
        {
            container.AddChild(unitInstance);
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
        var worldPos = gameArea.getGlobalFromTile(cell)+DEFAULT_OFFSET;
        return spawnUnitWithPosition(worldPos, unitInfo);
    }
}
