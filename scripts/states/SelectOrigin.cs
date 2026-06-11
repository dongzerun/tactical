using Godot;
using System;
using System.Collections.Generic;

public partial class SelectOrigin : BaseState
{
    public SkillState skillStateMachine;
    private Vector2I lastMouseCellPos = new Vector2I(-999,-999);
    public override void OnEnter()
    {
        GD.Print("SelectOrigin enter");
        skillStateMachine = parentFSM as SkillState;
        lastMouseCellPos = new Vector2I(-999,-999);
    }

    public override void OnExit()
    {
        battleNode.rangeSelector.ClearRange("origin_preview");
    }

    public override void StateInput(InputEvent @event)
    {
        if (@event.IsActionPressed("mouse_left"))
        {
            var cellPos = battleNode.gameArea.getHoveredTile();
            if (skillStateMachine.IsInCastRange(cellPos))
            {
                skillStateMachine.SetOriginPos(cellPos);
                GD.Print("SelectOrigin select origin " + cellPos);
                parentFSM.changeState("GetSkillRange");
            }
            else
            {
                GD.Print("SelectOrigin invalid pos");
            }
        }

        if (@event.IsActionPressed("mouse_right"))
        {
            parentFSM.parentFSM.changeState("AttackState");
        }
    }

    public override void StateProcess(float delta)
    {
        var cellPos = battleNode.gameArea.getHoveredTile();
        if (cellPos == lastMouseCellPos)
            return;
        
        lastMouseCellPos = cellPos;
        if (skillStateMachine.IsInCastRange(cellPos))
        {
            battleNode.rangeSelector.ShowRange(new List<Vector2I>{cellPos},new Color(0.4f,0.4f,0.4f),"origin_preview");
        }
        else
        {
            battleNode.rangeSelector.ClearRange("origin_preview");
        }
    }
}
