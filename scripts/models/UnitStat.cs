using Godot;
using System;
using System.Collections.Generic;

public partial class UnitStat : Resource
{
    [Export] private int MovePoint = 8;
    [Export] private int attackRange = 2;
    [Export] private int maxHP=1;
    [Export] private int attackDamage = 10;
    [Export] private int defense = 1;
    
    public static Dictionary<Terrain, int> moveCostMap = new Dictionary<Terrain, int>
    {
        {Terrain.LAND, 1},
        {Terrain.GRASS ,1},
        {Terrain.STONE, 2},
        {Terrain.RIVER ,-1},
    };

    public int GetMovePoint()
    {
        return MovePoint;
    }

    public int GetAttackDamage()
    {
        return attackDamage;
    }
    
    public int GetDefense()
    {
        return defense;
    }
    
    public int GetAttackRange()
    {
        return attackRange;
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetMovePoints()
    {
        return MovePoint;
    }

    public int GetMoveCost(Terrain terrain)
    {
        if (moveCostMap.ContainsKey(terrain))
            return moveCostMap[terrain];
        return -1;
    }
}
