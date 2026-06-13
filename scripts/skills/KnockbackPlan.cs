using Godot;
using System;

public enum CollisionType
{
    NONE,
    OBSTACLE,
    UNIT,
}

public partial class KnockbackPlan : Node
{
    public Vector2I StartCell = new Vector2I(-999, -999);
    public Vector2I LandingCell =  new Vector2I(999, 999);
    public CollisionType CollisionType = CollisionType.NONE;
    public Vector2I CollisionCell = new Vector2I(999, 999);
    public Unit CollisionUnit;
    public bool IsFalling;

    public override string ToString()
    {
        return $"StartCell {StartCell}, LandingCell {LandingCell}, CollisionType {CollisionType}, CollisionCell {CollisionCell}, IsFalling {IsFalling}";
    }

    public void SetData(Vector2I startCell, Vector2I landingCell,CollisionType collisionType,Vector2I collisionCell, Unit collisionUnit,bool isFalling)
    {
        StartCell = startCell;
        LandingCell = landingCell;
        CollisionType = collisionType;
        CollisionCell = collisionCell;
        CollisionUnit = collisionUnit;
        IsFalling = isFalling;
    }

    public bool HasCollision()
    {
        return CollisionType != CollisionType.NONE;
    }

    public bool IsUnitCollision()
    {
        return CollisionType == CollisionType.UNIT;
    }

    public bool IsObstacleCollision()
    {
        return CollisionType == CollisionType.OBSTACLE;
    }

    public bool IsValid()
    {
        return StartCell.X != -999 && StartCell.Y != -999;
    }
}
