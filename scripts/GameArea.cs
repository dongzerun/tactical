using Godot;
using System;

public partial class GameArea : TileMapLayer
{
    [Export] public GameGrid gameGrid;
    public Vector2I getTileFromGlobal(Vector2 global)
    {
        return LocalToMap(ToLocal(global));
    }

    public Vector2 getGlobalFromTile(Vector2I tile)
    {
        return ToLocal(MapToLocal(tile));
    }

    public Vector2I getHoveredTile()
    {
        return LocalToMap(GetLocalMousePosition());
    }
}
