using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class EnemyState : BaseState
{
    private Unit mainUnit;

    public override void OnEnter()
    {   
        GD.Print("EnemyState OnEnter");
        mainUnit = battleNode.GetMainUnit();
        if (mainUnit.faction != Faction.Enemy)
        {
            parentFSM.changeState("MoveState");
            return;
        }
        
        ExecuteAITurn();
    }

    public override void OnExit()
    {
        GD.Print("EnemyState OnExit");
    }

    public async void ExecuteAITurn()
    {
        battleNode.gridCalculator.initializeAstar(mainUnit);
        var targets = getEnemyUnits();
        if (targets.Count > 0)
        {
            var bestPath = calculateBestMovePath(targets);
            GD.Print("ExecuteAITurn targets: " + targets.Count + " bestPath: " + bestPath.Count);
            if (bestPath.Count > 1)
            {
                GD.Print("ExecuteAITurn paths " + bestPath.ToArray().Join(" "));
                battleNode.HideSkull();
                await battleNode.mover.MoveUnit(mainUnit, bestPath);
                battleNode.ShowSkullOnUnit(mainUnit);
            }
        }
        GD.Print("ExecuteAITurn try to attack targets: " + targets.Count);
        tryAttack();
        parentFSM.parentFSM.changeState("EndState");
    }

    private List<Vector2I> getNearbyPostions(Vector2I pos, int range)
    {
        List<Vector2I> results = new();
        foreach (var kvp in Unit.DIR_MAP)
        {
            foreach (var i in Enumerable.Range(-range, 2 * range + 1))
            {
                // skip current cell
                if (i==0)
                    continue;

                var nearby = pos + i*kvp.Key;
                if (!battleNode.gameArea.gameGrid.gridDB.ContainsKey(nearby))
                    continue;
                
                var gridData = battleNode.gameArea.gameGrid.gridDB[nearby];
                if (gridData.unit == null && gridData.obstacle == Obstacle.NULL)
                {
                    results.Add(nearby);
                    //GD.Print("getNearbyPostions " + pos + " nearby " + nearby);
                }
            }   
        }
       // GD.Print("getNearbyPostions " + pos + " cells " + results.Count);
        return results;
    }

    private List<Vector2I> calculateBestMovePath(List<Unit> targets)
    {
        var bestPath = new List<Vector2I>();
        var minDist = 999;
        var isBestReachable = false;
        var myPos = battleNode.gameArea.gameGrid.getUnitPosition(mainUnit);

        foreach (var target in targets)
        {
            var targetPos=battleNode.gameArea.gameGrid.getUnitPosition(target);
            if (targetPos == new Vector2I(-999, -999))
                continue;
            
            var nearbyCells = getNearbyPostions(targetPos, 1);
            if (nearbyCells.Count == 0)
                continue;

            var nearbyPos = getFirstAvailableNearbyPos(myPos, targetPos, nearbyCells, filter: pos => {
                // 检查位置是否有效
                if (!battleNode.gameArea.gameGrid.gridDB.ContainsKey(pos))
                    return false;
        
                var gridData = battleNode.gameArea.gameGrid.gridDB[pos];
                return gridData.unit == null && gridData.obstacle == Obstacle.NULL;
            });
            if (nearbyPos == new Vector2I(-999, -999))
                continue;
            
            var result = battleNode.gridCalculator.getMovePath(mainUnit, nearbyPos);
            var reachable = result[GridCalculator.ReachablePath];
            var unreachable  = result[GridCalculator.UnreachablePath];
            //GD.Print("calculateBestMovePath to target " + targetPos + " with nearbyPos "+ nearbyPos + " " + reachable.Count + " " + unreachable.Count + " " + result.Count);
            var currentExecutePath = reachable;
            if (currentExecutePath.Count == 0)
                continue;
            
            var currentIsReachable = unreachable.Count==0 && reachable.Count>0;
            var dist = 0;
            if (currentIsReachable)
            {
                dist = reachable.Count;
            }
            else
            {
                dist = reachable.Count + unreachable.Count;
            }

            var isBetter = false;
            if (currentIsReachable && !isBestReachable)
            {
                isBetter = true;
            } else if (currentIsReachable == isBestReachable && dist < minDist)
            {
                isBetter = true;
            }

            if (isBetter)
            {
                bestPath = currentExecutePath;
                minDist = dist;
                isBestReachable = currentIsReachable;
            }
        }
        return bestPath;
    }

    private List<Unit> getEnemyUnits()
    {
        List<Unit> results = new();
        foreach (var u in battleNode.allUnitsResource.GetAllUnits())
        {
            if (u == mainUnit || !IsInstanceValid(u) || u.IsDead())
                continue;
            
            if (u.faction != mainUnit.faction)
                results.Add(u);
        }
        return results;
    }

    private Vector2I getFirstAvailableNearbyPos(Vector2I from, Vector2I to, List<Vector2I> nearbys, Func<Vector2I, bool> filter = null)
    {
        var result = new Vector2I(-999,-999);
        
        if (nearbys == null || nearbys.Count == 0)
            return result;
        
        Vector2I? bestPos = null;
        int bestDistance = int.MaxValue;
        
        foreach (var nearby in nearbys)
        {
            var direction = nearby - from;
            // 应用过滤器
            if (filter != null && !filter(nearby))
                continue;
            
            // 计算这个 nearby 到目标 to 的距离（使用 Manhattan）
            var dist = Math.Abs(nearby.X - to.X) + Math.Abs(nearby.Y - to.Y);
            
            var nearbyDir = to-nearby;
            var dotProduct = nearbyDir.X * direction.X + nearbyDir.Y * direction.Y;
            
            if (dotProduct >= 0 && dist < bestDistance)
            {
                bestDistance = dist;
                bestPos = nearby;
            }
        }
        
        if (bestPos.HasValue)
            result = bestPos.Value;
        
        //GD.Print($"getFirstAvailableNearbyPos: from={from}, to={to}, best={result}, distance={bestDistance}");
        return result;
    }

    private async void tryAttack()
    {
        var unitPos = battleNode.gameArea.gameGrid.getUnitPosition(mainUnit);
        var attackRange = mainUnit.GetAttackRange();
        var attackableCells = battleNode.rangeCalculator.GetRangeCells(
            unitPos, attackRange);
        //GD.Print("tryAttack my pos " + unitPos + " attackRange " + attackRange + " attackableCells: " + attackableCells.ToArray().Join(" "));
        Unit bestTarget = null;
        foreach (var cell in attackableCells)
        {
            if (!battleNode.gameArea.gameGrid.gridDB.ContainsKey(cell))
                continue;
            
            var cellData = battleNode.gameArea.gameGrid.gridDB[cell];
            if (cellData == null)
                continue;
            
            //GD.Print("tryAttack cellData: " + cell);
            Unit targetUnit = cellData.unit as Unit;
            if (targetUnit == null || targetUnit is not Unit)
                continue;
            //GD.Print("tryAttack cellData: " + cell + " targetUnit 1" );
            if (targetUnit == mainUnit)
                continue;
            //GD.Print("tryAttack cellData: " + cell + " targetUnit 2" );
            if (targetUnit.faction == mainUnit.faction)
                continue;
            //GD.Print("tryAttack cellData: " + cell + " targetUnit 3" );
            if(bestTarget == null || targetUnit.currentHP < bestTarget.currentHP)
            {
                bestTarget = targetUnit;
            }
        }

        if (bestTarget == null)
        {
            //GD.Print("tryAattack bestTarget is null");
            return;
        }

        await battleNode.attackProcessor.TriggerAttack(mainUnit, bestTarget);
    }
}
