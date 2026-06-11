using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitResource : Resource
{
    public Unit unit;
    public BattleUnit battleUnit;

    public UnitResource(Unit _unit, BattleUnit _battleUnit)
    {
        unit = _unit;
        battleUnit = _battleUnit;
    }
}

public partial class AllUnits : Resource
{
    [Export] public int CurrentUnitIndex = 0;
    [Export] public Godot.Collections.Dictionary<int, UnitResource> UnitsDict = new();

    public int GetCount()
    {
        return UnitsDict.Count;
    }

    public Unit GetUnitByIndex(int index)
    {
        if (UnitsDict.ContainsKey(index))
            return UnitsDict[index].unit;
        return null;
    }

    public BattleUnit GetBattleUnitByIndex(int index)
    {
        if (UnitsDict.ContainsKey(index))
            return UnitsDict[index].battleUnit;
        return null;
    }

    public void AddUnit(Unit _unit, BattleUnit _battleUnit)
    {
        var key = UnitsDict.Count+1;
        UnitsDict[key-1] = new UnitResource(_unit, _battleUnit);
    }

    public void SetUnitAtIndex(int index, Unit _unit, BattleUnit _battleUnit)
    {
        UnitsDict[index] = new UnitResource(_unit, _battleUnit);
    }

    public void RemoveUnitAndUpdateIndex(int diedIndex)
    {
        var key = diedIndex;
        if (UnitsDict.ContainsKey(key))
        {
            UnitsDict.Remove(key);
            rebuildDictOrder();
        }

        if (diedIndex <= CurrentUnitIndex)
        {
            GD.Print($"Removing unit: {key}" + $" before index: {CurrentUnitIndex} count: " + GetCount());
            CurrentUnitIndex = Math.Max(0, CurrentUnitIndex-1);
        }
        GD.Print($"Removing unit: {key}" + $" current index: {CurrentUnitIndex} count: " + GetCount());
    }

    public void rebuildDictOrder()
    {
        Godot.Collections.Dictionary<int, UnitResource> tempUnits = new();
        int idx = 0;
        var sortedKeys = UnitsDict.Keys.OrderBy(k => k).ToList();
        foreach (var key in sortedKeys)
        {
            tempUnits.Add(idx++, UnitsDict[key]);
        }
        UnitsDict.Clear();
        UnitsDict = tempUnits;
    }

    public int SwitchToNext()
    {
        if (GetCount() == 0)
            return CurrentUnitIndex;

        // 获取所有存活的单位
        var aliveUnits = new List<Unit>();
        for (int i = 0; i < GetCount(); i++)
        {
            var unit = GetUnitByIndex(i);
            if (unit != null && !unit.IsDead())
            {
                aliveUnits.Add(unit);
            }
        }
        
        if (aliveUnits.Count == 0)
        {
            GD.Print("SwitchToNext: no alive units found");
            return CurrentUnitIndex;
        }
        
        // attempt size of units times to find live unit
        for (int i = 0; i < GetCount(); i++)
        {
            CurrentUnitIndex = (CurrentUnitIndex + 1) % GetCount();
            GD.Print($"Unit attempt {i} time to switch to {CurrentUnitIndex} current count " + GetCount());
            var unit = GetUnitByIndex(CurrentUnitIndex);
            if (unit != null && !unit.IsDead())
            {
                return CurrentUnitIndex;
            }
        }
        
        GD.Print("SwitchToNext: fallback, no live unit found after all attempts");
        return CurrentUnitIndex;
    }

    public List<Unit> GetAllUnits()
    {
        List<Unit> results=new();
        for (int i = 0; i < GetCount(); i++)
        {
            var unit = GetUnitByIndex(i);
            if (unit != null)
                results.Add(unit);
        }
        return results;
    }
    
    public AllUnits Clone()
    {
        var cloned = new AllUnits();
        cloned.CurrentUnitIndex = this.CurrentUnitIndex;
        cloned.UnitsDict = new Godot.Collections.Dictionary<int, UnitResource>();
    
        foreach (var kvp in this.UnitsDict)
        {
            BattleUnit clonedBattleUnit=new();
            var unit = kvp.Value.unit;
            var bunit = kvp.Value.battleUnit;
            if (bunit != null)
            {
                clonedBattleUnit.SetData(bunit.unitStat, bunit.faction,bunit.cellPos,bunit.direction, bunit.currentHP);
            }
            
            var clonedResource = new UnitResource(unit, clonedBattleUnit);
            cloned.UnitsDict[kvp.Key] = clonedResource;
        }
    
        return cloned;
    }

    public void UpdateBattleUnit(Func<Unit, Vector2I> cellPosGetter)
    {
        foreach (var key in UnitsDict.Keys)
        {
            var unitData = UnitsDict[key];
            var unit = unitData.unit;
            if (unit != null)
            {
                var cellPos = cellPosGetter(unit);
                if (cellPos is Vector2I pos)
                {
                    var newBunit = unit.CreateBattleUnit(pos);
                    UnitsDict[key].battleUnit = newBunit;   
                }
            }
        }
    }

    public Unit GetMainUnit()
    {
        return GetUnitByIndex(CurrentUnitIndex);
    }
}
