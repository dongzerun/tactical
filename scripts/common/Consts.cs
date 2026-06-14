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
    
    public static string ReachablePath = "reachable";
    public static string UnreachablePath = "unreachable";

    public static string PathPainterGroupDefault = "default";
    public static string PathPainterGroupReachable = "reachable";
    public static string PathPainterGroupUnreachable = "unreachable";
    
    public static string RangeSelectGroupDefault = "default";
    public static string RangeSelectGroupMove = "moveRange";
    public static string RangeSelectGroupAttack = "attackRange";
    public static string RangeSelectGroupOriginPreview = "originPreview";
    public static string RangeSelectGroupSkillCast = "skillCast";
    public static string RangeSelectGroupSkillPreview = "skillPreview";
}

