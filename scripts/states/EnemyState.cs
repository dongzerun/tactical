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
        foreach (var x in Enumerable.Range(-range, 2 * range + 1))
        {
            foreach (var y in Enumerable.Range(-range, 2 * range + 1))
            {
                if (x == 0 && y==0)
                    continue;

                var nearby = new Vector2I(x+pos.X, y+pos.Y);
                var gridData = battleNode.gameArea.gameGrid.gridDB[nearby];
                if (gridData.unit == null && gridData.obstacle == Obstacle.NULL)
                {
                    results.Add(nearby);
                    GD.Print("getNearbyPostions " + pos + " nearby " + nearby);
                }
            }
        }
        GD.Print("getNearbyPostions " + pos + " cells " + results.Count);
        return results;
    }

    private List<Vector2I> calculateBestMovePath(List<Unit> targets)
    {
        var bestPath = new List<Vector2I>();
        var minDist = 999;
        var isBestReachable = false;

        foreach (var target in targets)
        {
            var pos=battleNode.gameArea.gameGrid.getUnitPosition(target);
            if (pos == new Vector2I(-999, -999))
                continue;
            
            var nearbyPos = getNearbyPostions(pos, 1);
            if (nearbyPos.Count == 0)
                continue;
            
            // to simplfy, just pickup first pos
            var result = battleNode.gridCalculator.getMovePath(mainUnit, nearbyPos[0]);
            var reachable = result[GridCalculator.ReachablePath];
            var unreachable  = result[GridCalculator.UnreachablePath];
            GD.Print("calculateBestMovePath to target " + pos + " with nearbyPos "+ nearbyPos[0] + " " + reachable.Count + " " + unreachable.Count + " " + result.Count);
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

    private async void tryAttack()
    {
        var unitPos = battleNode.gameArea.gameGrid.getUnitPosition(mainUnit);
        var attackRange = mainUnit.GetAttackRange();
        var attackableCells = battleNode.rangeCalculator.GetRangeCells(
            unitPos, attackRange);
        GD.Print("tryAttack my pos " + unitPos + " attackRange " + attackRange + " attackableCells: " + attackableCells.ToArray().Join(" "));
        Unit bestTarget = null;
        foreach (var cell in attackableCells)
        {
            if (!battleNode.gameArea.gameGrid.gridDB.ContainsKey(cell))
                continue;
            
            var cellData = battleNode.gameArea.gameGrid.gridDB[cell];
            if (cellData == null)
                continue;
            
            GD.Print("tryAttack cellData: " + cell);
            Unit targetUnit = cellData.unit as Unit;
            if (targetUnit == null || targetUnit is not Unit)
                continue;
            GD.Print("tryAttack cellData: " + cell + " targetUnit 1" );
            if (targetUnit == mainUnit)
                continue;
            GD.Print("tryAttack cellData: " + cell + " targetUnit 2" );
            if (targetUnit.faction == mainUnit.faction)
                continue;
            GD.Print("tryAttack cellData: " + cell + " targetUnit 3" );
            if(bestTarget == null || targetUnit.currentHP < bestTarget.currentHP)
            {
                bestTarget = targetUnit;
            }
        }

        if (bestTarget == null)
        {
            GD.Print("tryAattack bestTarget is null");
            return;
        }

        await battleNode.attackProcessor.TriggerAttack(mainUnit, bestTarget);
    }
}
