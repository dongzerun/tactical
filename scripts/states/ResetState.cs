using Godot;
using System;
using System.Collections.Generic;

public partial class ResetState : BaseState
{
    private List<Unit> inactiveUnitsPool = new();

    public override void OnEnter()
    {
        GD.Print("ResetState enter");
        ExecuteReset();
    }

    public void ExecuteReset()
    {
        if (battleNode.backupAllUnitsResource == null)
        {
            GD.Print("ResetState: battleNode.backupAllUnitsResource is null, no need Reset");
            parentFSM.changeState("StartState");
            return; 
        }
        
        CollectInActiveUnits();
        RestoreUnitsFromBackup();
        CleanupInactivePools();
        FinalizeReset();
        parentFSM.changeState("StartState");
    }

    public void CollectInActiveUnits()
    {
        var allUnits = battleNode.allUnitsResource.GetAllUnits();
        foreach (var unit in allUnits)
        {
            unit.Hide();
            
            var cellPos = battleNode.gameArea.gameGrid.getUnitPosition(unit);
            battleNode.gameArea.gameGrid.removeUnitInMap(cellPos);
            inactiveUnitsPool.Add(unit);
        }
    }

    public Unit GetOrCreateUnit(BattleUnit bunit)
    {
        for (int i = 0; i < inactiveUnitsPool.Count; i++)
        {
            var poolUnit = inactiveUnitsPool[i];
            if (poolUnit.unitStat == bunit.unitStat)
            {
                inactiveUnitsPool.RemoveAt(i);
                return poolUnit;
            }
        }

        UnitInfo ui=new UnitInfo();
        ui.UnitStat=bunit.unitStat;
        ui.faction=bunit.faction;
        return battleNode.unitSpawner.SpawnUnitInCell(bunit.cellPos,ui);
    }

    public void RestoreUnitsFromBackup()
    {
        var backup = battleNode.backupAllUnitsResource;
        var backupCount = backup.GetCount();

        for (int i = 0; i < backupCount; i++)
        {
            BattleUnit bunit = backup.GetBattleUnitByIndex(i);
            if (bunit==null)
                continue;

            var unit = GetOrCreateUnit(bunit);
            if (unit != null)
            {
                unit.RestoreFromBattleUnit(bunit);
                unit.GlobalPosition = battleNode.gameArea.getGlobalFromTile(bunit.cellPos) + UnitSpawner.DEFAULT_OFFSET;
                unit.Show();
                
                unit.PlayIdle();
                battleNode.gameArea.gameGrid.addUnitInMap(unit, bunit.cellPos);
                backup.SetUnitAtIndex(i, unit, bunit);
            }
        }
    }

    public void CleanupInactivePools()
    {
        foreach (var unit in inactiveUnitsPool)
        {
            unit.QueueFree();
        }
        inactiveUnitsPool.Clear();
    }

    public void FinalizeReset()
    {
        battleNode.allUnitsResource = battleNode.backupAllUnitsResource;
        battleNode.ClearBackup();
    }
}
