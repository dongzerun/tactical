using Godot;
using System;

public partial class BaseState : Node
{
    [Export] public Battle battleNode;
    [Export] public BaseStateMachine parentFSM;
    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void StateProcess(float delta)
    {
    }

    public virtual void StateInput(InputEvent @event)
    {
    }
}
