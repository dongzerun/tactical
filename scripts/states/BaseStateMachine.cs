using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

public partial class BaseStateMachine : BaseState
{
    [Export] public string initialStateName = "InitState";
    // <from, to>
    public event Action<BaseState,BaseState> OnStateChanged;
    
    public BaseState currentState;
    public Dictionary<String, BaseState> states = new();

    public void Initialize(Battle battle)
    {
        battleNode = battle;
        states.Clear();

        foreach (var child in GetChildren())
        {
            if (child is BaseState state)
            {
                states[state.Name] = state;
                GD.Print("Register state: " + state.Name);
                state.parentFSM = this;
                if (child is BaseStateMachine bsm)
                {
                    bsm.Initialize(battleNode);
                }
                else
                {
                    state.battleNode = battleNode;
                }
            }
        }
    }

    public override void OnEnter()
    {
        if (currentState == null)
        {
            if (states.ContainsKey(initialStateName))
            {
                changeState(initialStateName);
            } else if (states.Count != 0)
            {
                changeState(states.Keys.First());
            }
        }
    }

    public override void OnExit()
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState = null;
        }
    }

    public override void StateInput(InputEvent @event)
    {
        if (currentState != null)
            currentState.StateInput(@event);
    }

    public override void StateProcess(float delta)
    {
        if (currentState != null)
            currentState.StateProcess(delta);
    }

    public void changeState(string stateName)
    {
        if (!states.ContainsKey(stateName))
        {
            GD.Print("change from "+currentState?.Name + " to state " + stateName + " doesn't exist");
            return;
        }
        var newState = states[stateName];
        if (currentState == newState)
            return;
        
        var previousState = currentState;
        if(currentState!=null)
            currentState.OnExit();
        
        currentState = newState;
        GD.Print("change state from " +  previousState?.Name + " to " + newState?.Name);
        currentState.OnEnter();
        OnStateChanged?.Invoke(currentState, previousState);
    }
}
