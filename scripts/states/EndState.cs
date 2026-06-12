using Godot;
using System;

public partial class EndState : BaseState
{
    public override void OnEnter()
    {
        parentFSM.changeState(StatsConst.SwitchState);
    }
    
    public override void OnExit()
    {
        GD.Print($"{StatsConst.EndState} OnExit");
        battleNode.HideSkull();
    }
}
