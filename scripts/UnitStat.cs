using Godot;
using System;
using System.Collections.Generic;

public partial class UnitStat : Resource
{
    [Export] public int MovePoint = 8;
    [Export] public int attackRange = 2;
    [Export] public int maxHP=1;
    [Export] public int attackDamage = 10;
    [Export] public int defense = 1;
    
    public static Dictionary<Terrain, int> moveCostMap = new Dictionary<Terrain, int>
    {
        {Terrain.LAND, 1},
        {Terrain.GRASS ,1},
        {Terrain.STONE, 2},
        {Terrain.RIVER ,-1},
    }; 
    
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
