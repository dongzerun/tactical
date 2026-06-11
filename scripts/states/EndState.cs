using Godot;
using System;

public partial class EndState : BaseState
{
    public override void OnEnter()
    {
        parentFSM.changeState("SwitchState");
    }
    
    public override void OnExit()
    {
        GD.Print("End OnExit");
        battleNode.HideSkull();
    }
}
