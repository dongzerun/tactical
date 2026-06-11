using Godot;
using System;

public partial class StartState : BaseState
{
    public override void OnEnter()
    {
        battleNode.BackupAllUnits();
        battleNode.gridCalculator.initializeAstar(battleNode.allUnitsResource.GetMainUnit());
        battleNode.ShowSkullOnUnit(battleNode.GetMainUnit());
        parentFSM.changeState("MainState");
    }
    
    public override void OnExit()
    {
        GD.Print("Start OnExit");
    }
}
