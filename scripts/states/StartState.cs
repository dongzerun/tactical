using Godot;
using System;

public partial class StartState : BaseState
{
    public override void OnEnter()
    {
        battleNode.BackupAllUnits();
        var mainUnit = battleNode.GetMainUnit();
        battleNode.gridCalculator.initializeAstar(mainUnit);
        battleNode.ShowSkullOnUnit(mainUnit);
        parentFSM.changeState(StatsConst.MainState);
    }
    
    public override void OnExit()
    {
        GD.Print("Start OnExit");
    }
}
