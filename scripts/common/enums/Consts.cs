using Godot;
using System;
using Godot.Collections;

public class Consts
{
    public static readonly System.Collections.Generic.Dictionary<Vector2I, Direction> DIR_MAP = new System.Collections.Generic.Dictionary<Vector2I, Direction>
    {
        { new Vector2I(0, -1), Direction.NE },
        { new Vector2I(-1, 0), Direction.NW },
        { new Vector2I(1, 0), Direction.SE },
        { new Vector2I(0, 1), Direction.SW }
    };
}

