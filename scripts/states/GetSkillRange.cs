using Godot;
using System;

public partial class GetSkillRange : BaseState
{
    public SkillState skillStateMachine;
    private Vector2I lastMouseCellPos = new Vector2I(-999,-999);
    public Vector2I PreviewDirection = new Vector2I(1,0);
    
    public override void OnEnter()
    {
        GD.Print("GetSkillRange OnEnter");
        skillStateMachine = parentFSM as SkillState;
        lastMouseCellPos = new Vector2I(-999,-999);
        PreviewDirection = new Vector2I(1,0);
    }

    public override void OnExit()
    {
    }

    public override void StateInput(InputEvent @event)
    {
        if (@event.IsActionPressed("mouse_left"))
        {
            GD.Print("GetSkillRange trigger try to execute skill");
            TryExecuteSkill();
        }
    }

    public override void StateProcess(float delta)
    {
        var skill = skillStateMachine.GetCurrentSkill();
        if (skill == null)
            return;

        var cellPos = battleNode.gameArea.getHoveredTile();
        if (cellPos == lastMouseCellPos)
            return;
        lastMouseCellPos = cellPos;
        
        UpdatePreviewDirection(skill, cellPos);
        var targetPos = GetTargetPos(skill, cellPos);
        var direction = skill.IsDirection ? PreviewDirection : Vector2I.Zero;
        var areaCells = skill.GetSkillAreaCells(targetPos, direction,battleNode.rangeCalculator);
        battleNode.rangeSelector.ShowRange(areaCells,new Color(1f,0.45f,0.35f,0.55f),"skill_preview");
    }

    public Vector2I GetTargetPos(BaseSkill skill, Vector2I cellPos)
    {
        if (skillStateMachine.NeedOriginSelection())
        {
            return skillStateMachine.GetOriginPos();
        } else if (skill.originType != OriginType.SELF)
        {
            return cellPos;
        }

        return skillStateMachine.GetCasterPos();
    }

    public void TryExecuteSkill()
    {
        var skill = skillStateMachine.GetCurrentSkill();
        var cellPos = battleNode.gameArea.getHoveredTile();
        UpdatePreviewDirection(skill, cellPos);
        if (skill.IsDirection)
        {
            skillStateMachine.Direction = PreviewDirection;
        }
        skillStateMachine.SetTargetPos(GetTargetPos(skill, cellPos));
        parentFSM.changeState(StatsConst.ExecuteSkill);
    }

    public void UpdatePreviewDirection(BaseSkill skill, Vector2I cellPos)
    {
        if (!skill.IsDirection)
            return;

        var directionSource = GetDirectionSource();
        var newDir = Utils.CalculateDirection(directionSource, cellPos);
        if (newDir != Vector2I.Zero)
            PreviewDirection = newDir;
    }

    public Vector2I GetDirectionSource()
    {
        if (skillStateMachine.NeedOriginSelection())
            return skillStateMachine.GetOriginPos();
        return skillStateMachine.GetCasterPos();
    }
}
