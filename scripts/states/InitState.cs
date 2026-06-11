using Godot;
using System;
using System.Collections.Generic;

public partial class InitState : BaseState
{
    public override void OnEnter()
    {
        GD.Print("InitState enter");
        initUnits();
        parentFSM.changeState("StartState");
    }

    public override void OnExit()
    {
        GD.Print("Init OnExit");
    }
    
    public void initUnits()
    {
        List<Unit> activeUnits = new();
        foreach (var side in battleNode.unitPosDict)
        {
            var pos = side.Key;
            var unitInfo = side.Value;
            var unit = battleNode.unitSpawner.SpawnUnitInCell(pos, unitInfo);
            if (!battleNode.gameArea.gameGrid.addUnitInMap(unit, pos))
            { 
                GD.Print($"Add unit {pos} in map failed"); 
                unit.QueueFree();
            }
            else 
            { 
                GD.Print($"Add unit {pos} in map succeeded"); 
                activeUnits.Add(unit);
            }
                
        }

        for (int i = 0; i < activeUnits.Count; i++)
        {
            var unit = activeUnits[i];
            var cellPos = battleNode.gameArea.gameGrid.getUnitPosition(unit);
            var bUnit = unit.CreateBattleUnit(cellPos);
            battleNode.allUnitsResource.SetUnitAtIndex(i, unit,bUnit);
        }

        battleNode.allUnitsResource.CurrentUnitIndex = 0;
        //battleNode.mainUnit=battleNode.activeUnits[battleNode.currentUnitIndex];
        //battleNode.mainUnit.setUnitColor(new Color(1f,0.5f,0.5f));
    }
}
