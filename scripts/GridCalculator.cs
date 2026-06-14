using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GridCalculator : Node
{
    [Export] private Battle _battle;

    private AStar2D _aStar = new();
    private List<Vector2I> _directions = new List<Vector2I>{
        Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right
    };

    private Unit _unit;
    private Dictionary<Vector2I, GridData> _gridDB = new();
    private Dictionary<Vector2I, int> _coordToID = new();

    public void initializeAstar(Unit unit)
    {
        _unit = unit;
        _aStar.Clear(); 
        _coordToID.Clear();

        if (_battle == null || _battle.gameArea == null || _battle.gameArea.gameGrid == null)
            return;

        _gridDB = _battle.gameArea.gameGrid.GetGridDB();
        GD.Print("initializeAstar again !!!");

        var idCounter = 0;
        // init all point in GridDB with cost
        foreach (var cell in _gridDB)
        {
            var cost = getMoveCost(UnitStat.moveCostMap, cell.Key);
            // GD.Print("initializeAstar " + cell.Key + " cost " + cost);
            if (cost != -1)
            {
                _aStar.AddPoint(idCounter, new Vector2(cell.Key.X,cell.Key.Y), cost);
                _coordToID[cell.Key] = idCounter++;
            }
        }

        // we only allow 4 directions move, so connect current cell to all avaiable 4 neighbor pos
        foreach (var cell in _coordToID)
        {
            foreach (var dir in _directions)
            {
                var neighborPos = cell.Key + dir;
                if (_coordToID.ContainsKey(neighborPos))
                {
                    _aStar.ConnectPoints(cell.Value, _coordToID[neighborPos]);
                }
            }
        }
    }
    
    /// <summary>
    /// 使用 Dijkstra 算法计算单位可以到达的所有格子
    /// 核心思想：从起点开始，逐步扩展到所有可达的格子，记录到达每个格子的最小移动消耗
    /// </summary>
    /// <param name="unit">要计算移动范围的单位</param>
    /// <returns>包含可达格子及其路径信息的结果</returns>
    public ReachableCellsInfo GetReachableCells(Unit unit)
    {
        // 1. 初始化结果对象
        ReachableCellsInfo results = new();
        
        // 2. 获取单位当前位置，如果不在地图上则返回空结果
        var startPos = _battle.GetUnitPosition(unit);
        if (!_coordToID.ContainsKey(startPos))
            return results;

        // 3. 获取单位的最大移动点数
        var maxMove = unit.GetMovePoints();
        
        // 4. 数据结构初始化：
        //    costSoFar: 记录到达每个格子的最小移动消耗
        //    parents: 记录到达每个格子的前一个格子（用于回溯路径）
        //    openList: 待处理的格子列表（存储 (消耗, 位置) 元组）
        var costSoFar = new Dictionary<Vector2I, int>
        {
            {startPos, 0}  // 起点消耗为0
        };
        var parents = new Dictionary<Vector2I, Vector2I>
        {
            {startPos, startPos}  // 起点的父节点是自己
        };
        var openList = new List<Tuple<int, Vector2I>>
        {
             new(0, startPos)  // 初始时只有起点
        };
        
        // 5. 获取移动消耗映射表（不同地形有不同消耗）
        var moveCostMap = UnitStat.moveCostMap;
        
        // 6. Dijkstra 主循环：处理所有待处理的格子
        while (openList.Count > 0)
        {
            // 6.1 找到消耗最小的格子（优先队列的简化实现）
            var minIndex = 0;
            var minCost = openList[0].Item1;
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].Item1 < minCost)
                {
                    minCost = openList[i].Item1;
                    minIndex = i;
                }
            }

            // 6.2 取出并移除当前消耗最小的格子
            var current = openList[minIndex];
            openList.RemoveAt(minIndex);
            var currentCost = current.Item1;
            var currentPos = current.Item2;

            // 6.3 如果当前记录的消耗已经比已知的最小消耗大，跳过（剪枝）
            if (currentCost > costSoFar[currentPos])
                continue;

            // 6.4 遍历四个方向的邻居格子
            foreach (var dir in _directions)
            {
                var nextPos = currentPos + dir;
                
                // 6.5 获取移动到邻居格子的消耗（-1表示不可通行）
                var moveCost = getMoveCost(moveCostMap, nextPos);
                if (moveCost == -1)
                    continue;

                // 6.6 计算到达邻居格子的总消耗
                var newCost = currentCost + moveCost;
                
                // 6.7 如果总消耗在移动点数范围内
                if (newCost <= maxMove)
                {
                    // 6.8 如果是新格子或找到更优路径
                    if (!costSoFar.ContainsKey(nextPos) || newCost < costSoFar[nextPos])
                    {
                        // 更新消耗、父节点，并加入待处理列表
                        costSoFar[nextPos] = newCost;
                        parents[nextPos] = currentPos;
                        openList.Add(new Tuple<int, Vector2I>(newCost, nextPos));
                    }
                }
            }
        }
        
        // 7. 填充结果并返回
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
            results.Add(curr);
            curr = parents[curr];
        }

        results.Add(curr);
        results.Reverse();
        return results;
    }

    public Dictionary<string, List<Vector2I>> GetMovePath(Unit unit, Vector2I targetPos)
    {
        var path = new Dictionary<string, List<Vector2I>>
        {
            {Consts.ReachablePath, new List<Vector2I>() },
            {Consts.UnreachablePath, new List<Vector2I>() }
        };
        var startPos = _battle.GetUnitPosition(unit);
        if (!_coordToID.ContainsKey(targetPos))
        {
            return path;
        }
        var targetId = _aStar.GetClosestPoint(targetPos);
        if (targetId == -1)
            return path;
        
        var idPaths = _aStar.GetIdPath(_coordToID[startPos],targetId);
        if (idPaths.Length == 0)
            return path;

        path[Consts.ReachablePath].Add(startPos);

        var currentMove = unit.GetMovePoints();
        var isReachable = true;
        for (int i = 1; i < idPaths.Length; i++)
        {
            var pos = (Vector2I)_aStar.GetPointPosition(idPaths[i]);
            if (isReachable)
            {
                var cost = getMoveCost(UnitStat.moveCostMap, pos);
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
        if (!_gridDB.ContainsKey(cell))
            return -1;
        
        var cellData = _gridDB[cell];
        if (cellData == null || cellData.obstacle!=Obstacle.NULL)
            return -1;

        if (cellData.unit != null && cellData.unit != _unit)
            return -1;

        var cost = map[cellData.terrain];
        if (cost <= 0)
            return -1; 
        return cost;
    }
}
