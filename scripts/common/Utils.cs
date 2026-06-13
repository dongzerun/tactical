using Godot;
using System;

public partial class Utils : Node
{
    public static Vector2I CalculateDirection(Vector2I from, Vector2I to)
    {
        var diff = to - from;
        if (diff == Vector2I.Zero)
        {
            return Vector2I.Zero;
        }

        if (Math.Abs(diff.X) > Math.Abs(diff.Y))
            return new Vector2I(Math.Sign(diff.X), 0);
        return new Vector2I(0, Math.Sign(diff.Y));
    }
}
