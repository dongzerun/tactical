using Godot;
using System;

public partial class GetCastRange : BaseState
{
    public SkillState skillStateMachine;

    public override void OnEnter()
    {
        GD.Print("GetCastRange OnEnter");
        skillStateMachine = parentFSM as SkillState;
        var skill = skillStateMachine.GetCurrentSkill();
        var caster = battleNode.GetMainUnit();
        var castRangeCells = skill.GetCastRangeCells(
            caster,
            battleNode,
            battleNode.rangeCalculator);
        skillStateMachine.SetCastRangeCells(castRangeCells);
        battleNode.rangeSelector.ShowRange(
            castRangeCells,
            new Color(0.2f,0.4f,1.0f,0.5f),
            Consts.RangeSelectGroupSkillCast);

        if (skillStateMachine.NeedOriginSelection())
        {
            GD.Print("GetCastRange need select origin");
            parentFSM.changeState(StatsConst.SelectOrigin);
        }
        else
        {
            GD.Print("GetCastRange no need select origin");
            var casterPos = battleNode.GetUnitPosition(caster);
            skillStateMachine.SetOriginPos(casterPos);
            parentFSM.changeState(StatsConst.GetSkillRange);
        }
    }
}
