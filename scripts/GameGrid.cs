using Godot;
using System;
using System.Collections.Generic;

public enum Terrain
{
    LAND,
    GRASS,
    STONE,
    RIVER,
}

public enum Obstacle
{
    ROCK,
    WOOD,
    NULL,
}

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
    
    public Dictionary<Vector2I,GridData> gridDB=new Dictionary<Vector2I,GridData>();
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
            var tileData = mainTileMap.GetCellTileData(cellPos);
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
            var tileData = obstacleTileMap.GetCellTileData(cellPos);
            var obstacleType = Obstacle.NULL;
            if (tileData != null)
            {
                var customTerrain = tileData.GetCustomData("obstacle");
                obstacleType = customTerrain.As<Obstacle>();
            }
            gridDB[cellPos].obstacle = obstacleType;
        }
    }
    
    public GridData getGridDB(Vector2I position)
    {
        return gridDB.ContainsKey(position) ? gridDB[position] : new GridData();
    }
    
    public bool addUnitInMap(Node2D unit, Vector2I cell)
    {
        if (!gridDB.ContainsKey(cell))
        {
            //GD.Print($"cell {cell} not available in tileMap");
            return false;
        }

        if (!isCellPosUsable(cell))
        {
            //GD.Print($"cell {cell} not usable temporaryly");
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

    public Vector2I getUnitPosition(Unit unit)
    {
        foreach (var kv in gridDB)
        {
            if (kv.Value.unit == unit)
                return kv.Key;
        }
        return new Vector2I(-999,-999);
    }

    private bool isCellPosUsable(Vector2I cell)
    {
        if (!gridDB.ContainsKey(cell))
            return false;
        
        var data = gridDB[cell];
        if (data.obstacle != Obstacle.NULL)
            return false;

        if (data.unit != null)
            return false;

        if (data.terrain == Terrain.RIVER)
            return false;
        return true;
    }
}
