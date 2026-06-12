using Godot;
using System;
using System.Collections.Generic;

public partial class SkillState : BaseStateMachine
{
    public BaseSkill CurrentSkill;
    public List<Vector2I> CastRangeCells=new();
    public Vector2I OriginPos=Vector2I.Zero;
    public Vector2I TargetPos = Vector2I.Zero;
    public Vector2I Direction =  Vector2I.Zero;

    public override void OnEnter()
    {
        ResetData();
        ClearVisuals();
        CurrentSkill = battleNode.GetCurrentSkill();
        if (CurrentSkill == null)
        {
            GD.Print("SkillStateMachine no available skill");
            parentFSM.changeState(StatsConst.AttackState);
            return;
        }
        base.OnEnter();
    }

    public override void OnExit()
    {
        ResetData();
        ClearVisuals();
        base.OnExit();
    }

    public void ResetData()
    {
        CurrentSkill = null;
        CastRangeCells.Clear();
        OriginPos = Vector2I.Zero;
        TargetPos = Vector2I.Zero;
        Direction = Vector2I.Zero;
    }

    public void SetCastRangeCells(List<Vector2I> rangeCells)
    {
        CastRangeCells = rangeCells;
    }

    public void SetOriginPos(Vector2I pos)
    {
        OriginPos = pos;
    }

    public Vector2I GetOriginPos()
    {
        return OriginPos;
    }

    public Vector2I GetCasterPos()
    {
        var caster = battleNode.GetMainUnit();
        if (caster == null)
            return Vector2I.Zero;
        return battleNode.gameArea.gameGrid.getUnitPosition(caster);
    }

    public void SetTargetPos(Vector2I pos)
    {
        TargetPos = pos;
    }

    public Vector2I GetTargetPos()
    {
        return TargetPos;
    }

    public bool NeedOriginSelection()
    {
        if (CurrentSkill == null)
            return false;
        return CurrentSkill.IsDirection && CurrentSkill.originType != OriginType.SELF;
    }

    public void ClearVisuals()
    {
        battleNode.rangeSelector.ClearAllRanges();
    }

    public bool IsInCastRange(Vector2I pos)
    {
        return CastRangeCells.Contains(pos);
    }

    public BaseSkill GetCurrentSkill()
    {
        //GD.Print("GetCurrentSkill "+CurrentSkill.SkillName);
        return CurrentSkill;
    }
}
