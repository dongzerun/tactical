using Godot;
using System;
using System.Collections.Generic;

public class GridData
{
    public Terrain terrain;
    public Obstacle obstacle;
    public Node2D unit;
}

public partial class GameGrid : Node
{
    [Export] private TileMapLayer mainTileMap;
    [Export] private TileMapLayer obstacleTileMap;

    public static Action gridChanged;
    
    private Dictionary<Vector2I,GridData> gridDB=new();
    public override void _Ready()
    {
        if (mainTileMap == null)
            return;
        initializeGrid();
    }

    private void initializeGrid()
    {
        gridDB.Clear();
        foreach (var cellPos in mainTileMap.GetUsedCells())
        {
            TileData tileData = mainTileMap.GetCellTileData(cellPos);
            var terrainType = Terrain.LAND;
            if (tileData != null)
            {
                var customTerrain = tileData.GetCustomData("terrain");
                terrainType = customTerrain.As<Terrain>();
            }
            gridDB[cellPos] = new GridData
            {
                unit = null,
                terrain = terrainType,
                obstacle = Obstacle.NULL,
            };
        }
        foreach (var cellPos in obstacleTileMap.GetUsedCells())
        {
            TileData tileData = obstacleTileMap.GetCellTileData(cellPos);
            var obstacleType = Obstacle.NULL;
            if (tileData != null)
            {
                var customTerrain = tileData.GetCustomData("obstacle");
                obstacleType = customTerrain.As<Obstacle>();
            }
            gridDB[cellPos].obstacle = obstacleType;
        }
    }
    
    public Dictionary<Vector2I,GridData> GetGridDB()
    {
        return gridDB;
    }
    
    public GridData GetGridData(Vector2I cell)
    {
        if(gridDB.ContainsKey(cell))
            return gridDB[cell];
        return null;
    }
    
    public bool addUnitInMap(Node2D unit, Vector2I cell)
    {
        if (!gridDB.ContainsKey(cell))
        {
            return false;
        }

        if (!isCellPosUsable(cell))
        {
            return false;
        }

        gridDB[cell].unit = unit;
        gridChanged?.Invoke();
        return true;
    }

    public bool removeUnitInMap(Vector2I cell)
    {
        if (!gridDB.ContainsKey(cell))
        {
            return false;
        }

        if (gridDB[cell].unit == null)
            return false;
        
        gridDB[cell].unit = null;
        gridChanged?.Invoke();
        return true;
    }

    private bool isCellPosUsable(Vector2I cell)
    {
        if (!gridDB.ContainsKey(cell))
            return false;
        
        var data = gridDB[cell];
        // can only be added when there's no obstacle in Grid
        if (data.obstacle != Obstacle.NULL)
            return false;

        if (data.unit != null)
            return false;

        // actually if we have flight unit, should be avaiable on RIVER
        // currently just ignore
        if (data.terrain == Terrain.RIVER)
            return false;
        return true;
    }
}
