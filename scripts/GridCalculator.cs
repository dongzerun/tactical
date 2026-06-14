using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GridCalculator : Node
{
    [Export] private Battle _battle;

    private AStar2D aStar = new();
    private List<Vector2I> directions = new List<Vector2I>{
        Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right
    };

    private Unit _unit;
    private Dictionary<Vector2I, GridData> gridData = new();
    private Dictionary<Vector2I, int> coordToID = new();

    public void initializeAstar(Unit unit)
    {
        _unit = unit;
        aStar.Clear(); 
        coordToID.Clear();

        if (_battle == null || _battle.gameArea == null || _battle.gameArea.gameGrid == null)
            return;

        gridData = _battle.gameArea.gameGrid.GetGridDB();
        GD.Print("initializeAstar again !!!");
        var moveCostMap = UnitStat.moveCostMap;
        var id_counter = 0;
        foreach (var cell in gridData)
        {
            var cost = getMoveCost(moveCostMap, cell.Key);
            // GD.Print("initializeAstar " + cell.Key + " cost " + cost);
            if (cost != -1)
            {
                aStar.AddPoint(id_counter,new Vector2(cell.Key.X,cell.Key.Y), cost);
                coordToID[cell.Key] = id_counter;
                id_counter++;
            }
        }

        foreach (var cell in coordToID)
        {
            foreach (var dir in directions)
            {
                var neighborPos = cell.Key + dir;
                if (coordToID.ContainsKey(neighborPos))
                {
                    aStar.ConnectPoints(cell.Value,coordToID[neighborPos]);
                }
            }
        }
    }
    
    public class ReachableCellsInfo
    {
        public Dictionary<Vector2I,int> CostSofar;
        public Dictionary<Vector2I, Vector2I> Parents;
    }

    public ReachableCellsInfo GetReachableCells(Unit unit)
    {
        ReachableCellsInfo results = new();
        var startPos = _battle.GetUnitPosition(unit);
        if (!coordToID.ContainsKey(startPos))
            return results;

        var maxMove = unit.GetMovePoints();
        
        var costSoFar = new Dictionary<Vector2I,int>
        {
            {startPos, 0}
        };
        var parents = new Dictionary<Vector2I, Vector2I>
        {
            {startPos, startPos }
        };
        
        var openList = new List<Tuple<int, Vector2I>>
        {
             new ( 0, startPos)
        };
        
        var moveCostMap = UnitStat.moveCostMap;
        //GD.Print("start Pos " +startPos + " maxMove "  +maxMove + "openList count " + openList.Count);
        while (openList.Count > 0)
        {
            var minIndex = 0;
            var minCost=openList[0].Item1;
            for (int i = 1; i < openList.Count; i++)
            {
                minCost=openList[i].Item1;
                minIndex = i;

                if (openList[i].Item1 < minCost)
                {
                    minCost = openList[i].Item1;
                    minIndex = i;
                }
            }

            var current = openList[minIndex];
            openList.RemoveAt(minIndex);
            var currentCost = current.Item1;
            var currentPos = current.Item2;

            if (currentCost > costSoFar[currentPos])
                continue;

            foreach (var dir in directions)
            {
                var nextPos = currentPos + dir;
                var moveCost=getMoveCost(moveCostMap, nextPos);
                if (moveCost == -1)
                    continue;

                var newCost = currentCost + moveCost;
                if (newCost <= maxMove)
                {
                    if (!costSoFar.ContainsKey(nextPos) || newCost < costSoFar[nextPos])
                    {
                        costSoFar[nextPos] = newCost;
                        parents[nextPos] = currentPos;
                        openList.Add(new Tuple<int, Vector2I>(newCost, nextPos));
                    }
                }
            }
        }
        results.CostSofar = costSoFar;
        results.Parents = parents;
        return results;
    }

    public List<Vector2I> GetTargetPath(Vector2I targetPos, Dictionary<Vector2I, Vector2I> parents)
    {
        var results=new List<Vector2I>();
        if (!parents.ContainsKey(targetPos))
            return results;

        var curr = targetPos;
        while (curr!=parents[curr])
        {
            //GD.Print("Get target Path " + targetPos + " curr "+curr + "parents " + parents[curr]);
            results.Add(curr);
            curr = parents[curr];
        }

        results.Add(curr);
        results.Reverse();
        return results;
    }

    public Dictionary<string, List<Vector2I>> getMovePath(Unit unit, Vector2I targetPos)
    {
        //initializeAstar(unit);
        var path = new Dictionary<string, List<Vector2I>>
        {
            {Consts.ReachablePath, new List<Vector2I>() },
            {Consts.UnreachablePath, new List<Vector2I>() }
        };
        var startPos = _battle.GetUnitPosition(unit);
        GD.Print("getMovePath start Pos " +startPos + " target " + targetPos);
        if (!coordToID.ContainsKey(targetPos))
        {
            GD.Print("getMovePath " + targetPos + " not in coordToID" + " length " + coordToID.Count);
            return path;
        }
        var targetID = aStar.GetClosestPoint(targetPos);
        //GD.Print("targetID2 "+targetID);
        if (targetID == -1)
            return path;
        
        var idPaths = aStar.GetIdPath(coordToID[startPos],targetID);
        //GD.Print("targetID3 "+targetID);
        if (idPaths.Length == 0)
            return path;
        //GD.Print("idPaths " +idPaths);
        path[Consts.ReachablePath].Add(startPos);

        var currentMove = unit.GetMovePoints();
        var isReachable = true;
        var moveCostMap = UnitStat.moveCostMap;
        for (int i = 1; i < idPaths.Length; i++)
        {
            var pos = (Vector2I)aStar.GetPointPosition(idPaths[i]);
            if (isReachable)
            {
                var cost = getMoveCost(moveCostMap, pos);
                if (cost != -1 && currentMove >= cost)
                {
                    currentMove -= cost;
                    path[Consts.ReachablePath].Add(pos);
                }
                else
                {
                    isReachable = false;
                    path[Consts.UnreachablePath].Add(pos);
                }
            }
            else
            {
                path[Consts.UnreachablePath].Add(pos);
            }
        }
        return path;
    }

    private int getMoveCost(Dictionary<Terrain, int> map, Vector2I cell)
    {
        if (!gridData.ContainsKey(cell))
            return -1;
        
        var cellData = gridData[cell];
        if (cellData == null || cellData.obstacle!=Obstacle.NULL)
            return -1;

        if (cellData.unit != null && cellData.unit != _unit)
        {
            GD.Print("GetMoveCocst -1 due to unit not empty " + cell);
            return -1;
        }

        var cost = map[cellData.terrain];
        if (cost <= 0)
            return -1; 
        return cost;
    }
}
