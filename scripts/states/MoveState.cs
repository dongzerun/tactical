using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MoveState : BaseState
{
    private bool isMoving = false;
    private bool showingArea = false;
    private Unit _mainUnit = null;
    public Dictionary<Vector2I,int> reachableCells=new();
    public Dictionary<Vector2I, Vector2I> parents=new();
    private Vector2I lastHoveredTile;

    public override void OnEnter()
    {
        GD.Print("MoveState enter");
        _mainUnit = battleNode.allUnitsResource.GetMainUnit();

        if (_mainUnit.faction == Faction.Enemy)
        {
            GD.Print("GOTO EnemyState");
            parentFSM.changeState(StatsConst.EnemyState);
            return;
        }
        
        battleNode.mover.MoveFinishedEvent += OnMoveFinished;
        showMoveArea();
    }
    
    public override void OnExit()
    {
        GD.Print("Move OnExit");
        // 退出时断开事件连接
        battleNode.mover.MoveFinishedEvent -= OnMoveFinished;
    }

    public override void StateProcess(float delta)
    {
        if (isMoving)
            return;
        
        var currentTile = battleNode.gameArea.getHoveredTile();
        if (currentTile == lastHoveredTile)
            return;
        
        //GD.Print("try to paint to target tile " + currentTile);
        lastHoveredTile = currentTile;
        var lastPos = updatePathPreview(currentTile);

        if (lastPos.X != -999 && lastPos.Y != -999)
        {
            var rangeVal = _mainUnit.GetAttackRange();
            var attackableCells = battleNode.rangeCalculator.GetRangeCells(lastPos, rangeVal);
            //GD.Print("ShowRange " + lastPos + " range " + rangeVal + " cells " + attackableCells.Count);
            battleNode.rangeSelector.ShowRange(attackableCells, new Color(1,0,0,0.5f),Consts.RangeSelectGroupAttack);
        }
        else
        {
            battleNode.rangeSelector.ClearRange(Consts.RangeSelectGroupAttack);
        }

    }
    
    public void showMoveArea()
    {
        if (isMoving)
            return;
        if (showingArea)
            return;

        var result = battleNode.gridCalculator.GetReachableCells(_mainUnit);
        reachableCells = result.CostSofar;
        parents = result.Parents;
        var cells =reachableCells.Keys;
        //GD.Print("reachableCells " + reachableCells.Keys.ToArray().Join(""));
        //GD.Print("parents " + parents.Count);
        battleNode.rangeSelector.ShowRange(cells.ToList(),new Color(0.4f,0.6f,1.0f,0.5f), Consts.RangeSelectGroupMove);
        showingArea = true;
    }

    public override void StateInput(InputEvent @event)
    {
        if (@event.IsActionPressed("reset"))
        {
            battleNode.RequestReset();
            return;
        }

        if (@event.IsActionPressed("stop_move"))
        {
            parentFSM.changeState(StatsConst.AttackState);
            return;
        }

        if( @event.IsActionPressed("mouse_left"))
        {
            var currentTile=battleNode.gameArea.getHoveredTile();
            if (_mainUnit == null)
                return;
            
            var result = battleNode.gridCalculator.GetMovePath(_mainUnit, currentTile);
            if (result["reachable"].Count > 0)
            {
                //GD.Print("Input move unit to " + currentTile + " with " + result["reachable"].ToArray().Join(","));
                battleNode.HideSkull();
                MoveUnit(result["reachable"]);
            }
            return;
        }
        
        // right click to clean show move area
        if (@event.IsActionPressed("mouse_right"))
        {
            if (isMoving)
                return;
            if (showingArea)
            {
                //GD.Print("mouse right again to clear showing area");
                battleNode.rangeSelector.ClearAllRanges();
                reachableCells.Clear();
                parents.Clear();
                showingArea = false;
            }
            else
            {
                showMoveArea();
            }
            return;
        }
    }

    public Vector2I updatePathPreview(Vector2I tile)
    {
        var lastVec = new Vector2I(-999, -999);
        var mainUnit = _mainUnit;
        var pathPainter = battleNode.pathPainter;
        var gridCalculator =  battleNode.gridCalculator;
        if (mainUnit == null)
            return lastVec;

        pathPainter.clearPath("");
        if (reachableCells.ContainsKey(tile))
        {
            var path=gridCalculator.GetTargetPath(tile, parents);
            //GD.Print("reachable in parents path " + path.Count);
            if (path.Count > 0)
            {
                pathPainter.ShowPath(path, new Color(1f,1f,0.5f,0.9f),Consts.PathPainterGroupReachable);
                pathPainter.clearPath(Consts.PathPainterGroupUnreachable);
            }

            lastVec = tile;
            return lastVec;
        }

        var result = gridCalculator.GetMovePath(mainUnit, tile);
        var reachable = result[Consts.PathPainterGroupReachable];
        var unreachable = result[Consts.PathPainterGroupUnreachable];
        //GD.Print("reachable " +reachable.Count);
        //GD.Print("unreachable " + unreachable.Count);
        if (reachable.Count > 0)
        {
            lastVec = reachable.Last();
            //GD.Print("lastVec " + lastVec);
            pathPainter.ShowPath(reachable, new Color(1f,1f,0.5f,0.9f),Consts.PathPainterGroupReachable);
        }
        else
        {
            pathPainter.clearPath(Consts.PathPainterGroupReachable);
        }

        if (unreachable.Count > 0)
        {
            if (reachable.Count > 0)
            {
                unreachable.Insert(0, reachable.Last());
            }
            pathPainter.ShowPath(unreachable, new Color(0.5f,0.5f,0.5f,1f),Consts.PathPainterGroupUnreachable);
        }
        else
        {
            pathPainter.clearPath(Consts.PathPainterGroupUnreachable);
        }
        
        return lastVec;
    }
    public async void MoveUnit(List<Vector2I> path)
    {
        isMoving = true;
        battleNode.pathPainter.clearPath("");
        battleNode.rangeSelector.ClearAllRanges();

        if (path.Count > 1)
        {
            await battleNode.mover.MoveUnit(_mainUnit,path);
        }
    }

    public void OnMoveFinished()
    {
        //GD.Print("received OnMoveFinished");
        isMoving = false;
        lastHoveredTile = new Vector2I(-999, -999);
        showingArea = false;
        reachableCells.Clear();
        parents.Clear();
        battleNode.gridCalculator.initializeAstar(_mainUnit);
        parentFSM.changeState(StatsConst.AttackState);
    }
}
