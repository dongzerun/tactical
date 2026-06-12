using Godot;
using System;
using System.Threading.Tasks;

public class TraceKnockbackPath
{
    public Vector2I FinalCell;
    public CollisionType CollisionType = CollisionType.NONE;
    public Vector2I collisionCell=new Vector2I(-999,-999);
    public Unit collisionUnit = null;
}

public class CollisionInfo
{
    public Unit CollUnit=null;
    public CollisionType CollType = CollisionType.NONE;
}

public partial class KnockbackProcessor : Node
{
    [Export] public GameArea gameArea;
    [Export] public AttackProcessor attackProcessor;
    [Export] public float DefaultPerCellTime = 0.1f;
    [Export] public float FallDistance = 64f;
    [Export] public float FallDuration = 0.2f;

    public async void ExecuteKnockback(Unit attacker, Unit defender, int distance = -1, float perCellTime = -0.1f)
    {
        if (!IsInstanceValid(attacker) || !IsInstanceValid(defender) || gameArea==null)
            return;
        
        var originPos = gameArea.gameGrid.getUnitPosition(attacker);
        await executeKnockbackInternal(originPos, defender, distance, perCellTime);
    }

    public async Task KnockbackUnit(Vector2I originPos, Unit target, int distance=-1, float perCellTime=-0.1f)
    {
        if (!IsInstanceValid(target))
            return;

        await executeKnockbackInternal(originPos, target, distance, perCellTime);
    }

    private async Task executeKnockbackInternal(Vector2I originPos, Unit target, int distance, float perCellTime)
    {
        var plan = planKnockbackPath(originPos, target, distance);
        if (!plan.IsValid())
            return;
        GD.Print("executeKnockbackInternal plan " + plan.ToString());
        var moveTime = perCellTime< 0 ? DefaultPerCellTime : perCellTime;
        if (gameArea != null && gameArea.gameGrid != null)
        {
            gameArea.gameGrid.removeUnitInMap(plan.StartCell);
        }

        if (!plan.IsFalling)
        {
            if (gameArea != null && gameArea.gameGrid != null)
            {
                gameArea.gameGrid.addUnitInMap(target, plan.LandingCell);
            }
            target.PlayIdle();
        }

        await playKnockbackAnimation(target, plan, moveTime);
        applyCollisionDamage(target, plan);
        if (plan.IsFalling)
        {
            await handleFalling(target);
        }
    }

    private async Task handleFalling(Unit unit)
    {
        // ensure shadowed by other terrain
        unit.ZIndex = -1;
        var finalPosition = new Vector2(unit.Position.X, unit.Position.Y + FallDistance);
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        tween.TweenProperty(unit, "position", finalPosition, FallDuration);
        tween.TweenProperty(unit, "modulate:a", 0.0f, FallDuration);
        await ToSignal(tween, "finished");

        if (IsInstanceValid(unit))
        {
            unit.Hide();
            unit.die();
        }
    }

    private void applyCollisionDamage(Unit defender, KnockbackPlan plan)
    {
        if (attackProcessor == null || !IsInstanceValid(defender) || !plan.HasCollision())
            return;

        var COLLISION_DAMAGE = 0.5;
        var damage = Math.Ceiling(defender.GetMaxHP() * COLLISION_DAMAGE);
        attackProcessor.ExecuteWorldDamage(defender, (int)damage);

        if (plan.IsUnitCollision() && IsInstanceValid(plan.CollisionUnit))
        {
            var otherDamage = Math.Ceiling(plan.CollisionUnit.GetMaxHP() * COLLISION_DAMAGE);
            attackProcessor.ExecuteWorldDamage(plan.CollisionUnit, (int)otherDamage);
        }
    }

    public async Task playKnockbackAnimation(Unit target, KnockbackPlan plan, float perCellTime)
    {
        if (!plan.HasCollision())
        {
            await playMoveAnimation(target,plan.StartCell, plan.LandingCell,perCellTime);
            return;
        }

        target.ZIndex = 1;
        await playMoveAnimation(target, plan.StartCell, plan.CollisionCell, perCellTime);
        await playBounceAnimation(target, plan.CollisionCell, plan.LandingCell, perCellTime);
        target.ZIndex = 0;
    }

    public async Task playMoveAnimation(Unit target, Vector2I start, Vector2I end, float perCellTime)
    {
        if (start == end)
            return;

        var distance = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        var duration = Math.Max(0.1f, perCellTime * (float)distance);
        target.PlayIdle();
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(target, "position", gameArea.getGlobalFromTile(end) + UnitSpawner.DEFAULT_OFFSET,duration);
        await ToSignal(tween, "finished");
    }

    public async Task playBounceAnimation(Unit target, Vector2I collision, Vector2I landing, float perCellTime)
    {
        if (collision == landing)
            return;
        
        target.PlayIdle();
        var duration = Math.Max(0.03f, perCellTime * 0.35);
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(target, "position", gameArea.getGlobalFromTile(landing) + UnitSpawner.DEFAULT_OFFSET,duration);
        await ToSignal(tween, "finished");
    }

    private KnockbackPlan planKnockbackPath(Vector2I originPos, Unit target, int distance)
    {
        var plan = new KnockbackPlan();
        var targetPos = gameArea.gameGrid.getUnitPosition(target);
        if (targetPos.X == -999 && targetPos.Y == -999)
            return plan;
        
        var direction = calculateKnockbackDirection(originPos, targetPos);
        if (direction == Vector2I.Zero)
            return plan;
        
        var pathResult = getTraceKnockbackPath(targetPos, direction, distance, target);
        plan.SetData(
            targetPos,
            pathResult.FinalCell,
            pathResult.CollisionType,
            pathResult.collisionCell,
            pathResult.collisionUnit,
            shouldFalling(pathResult.FinalCell,  pathResult.CollisionType));
        return plan;
    }

    private bool shouldFalling(Vector2I cell, CollisionType collType)
    {
        if (collType != CollisionType.NONE)
            return false;

        if (!gameArea.gameGrid.gridDB.ContainsKey(cell))
            return true;
        
        var cellData = gameArea.gameGrid.gridDB[cell];
        return (cellData == null || cellData.terrain == Terrain.RIVER);
    }

    private Vector2I calculateKnockbackDirection(Vector2I from, Vector2I to)
    {
        return new Vector2I(Math.Sign(to.X - from.X), Math.Sign(to.Y - from.Y));
    }

    private TraceKnockbackPath getTraceKnockbackPath(Vector2I startCell, Vector2I direction, int distance, Unit movingUnit)
    {
        var result = new TraceKnockbackPath();
        result.FinalCell = startCell;
        for (int i = 1; i < distance+1; i++)
        {
            var nextCell = startCell + direction * i;
            var collisionInfo = checkCollision(nextCell, movingUnit);
            if (collisionInfo.CollType != CollisionType.NONE)
            {
                result.collisionUnit = movingUnit;
                result.collisionCell = nextCell;
                result.CollisionType = collisionInfo.CollType;
                return result;
            }

            result.FinalCell = nextCell;
        }
        return result;
    }

    public CollisionInfo checkCollision(Vector2I cell, Unit ignoreUnit)
    {
        var result = new CollisionInfo();
        if (!gameArea.gameGrid.gridDB.ContainsKey(cell))
            return result;
        
        var cellData = gameArea.gameGrid.gridDB[cell];
        if (cellData == null)
            return result;

        if (cellData.obstacle != Obstacle.NULL)
        {
            result.CollType = CollisionType.OBSTACLE;
            return result;
        }

        var otherUnit = cellData.unit as Unit;
        if (otherUnit != null && otherUnit != ignoreUnit)
        {
            result.CollType = CollisionType.UNIT;
            result.CollUnit = otherUnit;
        }
        return result;
    }
}
