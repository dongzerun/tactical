using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class AttackState : BaseState
{
    private List<Vector2I> attackableCells;
    private bool isAttacking = false;
    
    public override void OnEnter()
    {
        GD.Print("Attack OnEnter");
        isAttacking = false;
        var unit = battleNode.allUnitsResource.GetMainUnit();
        battleNode.ShowSkullOnUnit(unit);
        GD.Print("attack onenter activeunits "+battleNode.allUnitsResource.GetCount() + " currindex " + battleNode.allUnitsResource.CurrentUnitIndex);
        GD.Print("attack onenter unit keys: " + battleNode.allUnitsResource.UnitsDict.Keys.ToArray().Join(","));
        if (unit == null)
        {
            throw new Exception("Unit not found, shouldn't happened");
        }

        var centerPos = battleNode.GetUnitPosition(unit);
        var rangeVal = unit.GetAttackRange();
        
        attackableCells = battleNode.rangeCalculator.GetRangeCells(centerPos, rangeVal,DistanceAlgorithm.MANHATTAN);
        battleNode.rangeSelector.ShowRange(attackableCells,new Color(1,0,0,0.5f),"attackRange");
    }
    
    public override void OnExit()
    {
        GD.Print("Attack OnExit");
        battleNode.rangeSelector.ClearRange("attackRange");
        isAttacking = false;
        
    }

    public override void StateInput(InputEvent @event)
    {
        if (isAttacking)
            return;

        if (@event.IsActionPressed("reset"))
        {
            battleNode.RequestReset();
            return;
        }
        
        if (@event is InputEventKey && @event.IsPressed())
        {
            var selectedSkill = battleNode.TrySelectSkillFromInput(@event);
            if (selectedSkill != null)
            {
                parentFSM.changeState(StatsConst.SkillState);
                return;
            }
        }
        
        if (@event.IsActionPressed("mouse_left"))
        {
            var currentTile = battleNode.gameArea.getHoveredTile();
            if (attackableCells.Contains(currentTile))
            {
                var cellData = battleNode.GetGridData(currentTile);
                if (cellData == null)
                    return;
                
                var targetUnit = cellData.unit as Unit;
                var attackUnit = battleNode.allUnitsResource.GetMainUnit();
                GD.Print("is valid target????" + IsInstanceValid(targetUnit));
                if (targetUnit!=null && targetUnit!=attackUnit)
                {
                    TriggerAttackSync(attackUnit, targetUnit);
                }
            }
        }

        if (@event.IsActionPressed("mouse_right"))
        {
            parentFSM.parentFSM.changeState(StatsConst.EndState);
        }
    }

    public async Task TriggerAttack(Unit attack, Unit defender)
    {
        isAttacking = true;
        var dir = GetAttackDirection(attack.Position, defender.Position);
        attack.PlayAttack(dir);
        
        var tcs = new TaskCompletionSource<bool>();
        
        // 使用一次性事件连接
        attack.attackTriggerDamage += OnAttackTriggerDamage;
        
        attack.animatedSprite2DNode.AnimationFinished += OnAttackAnimationFinished;
        
        void OnAttackTriggerDamage()
        {
            battleNode.attackProcessor.ExecuteAttack(attack, defender);
            // 断开事件连接
            attack.attackTriggerDamage -= OnAttackTriggerDamage;
        }
        
        void OnAttackAnimationFinished()
        {
            attack.PlayIdle();
            isAttacking = false;
            // 断开事件连接
            attack.animatedSprite2DNode.AnimationFinished -= OnAttackAnimationFinished;
            tcs.SetResult(true);
        }
        
        await tcs.Task;
    }
    
    // 同步版本，用于 StateInput
    public void TriggerAttackSync(Unit attack, Unit defender)
    {
        isAttacking = true;
        var dir = GetAttackDirection(attack.Position, defender.Position);
        attack.PlayAttack(dir);
        
        // 使用一次性事件连接
        attack.attackTriggerDamage += OnAttackTriggerDamage;
        
        attack.animatedSprite2DNode.AnimationFinished += OnAttackAnimationFinished;
        
        void OnAttackTriggerDamage()
        {
            battleNode.attackProcessor.ExecuteAttack(attack, defender);
            // 断开事件连接
            attack.attackTriggerDamage -= OnAttackTriggerDamage;
        }
        
        void OnAttackAnimationFinished()
        {
            attack.PlayIdle();
            isAttacking = false;
            // 断开事件连接
            attack.animatedSprite2DNode.AnimationFinished -= OnAttackAnimationFinished;
            parentFSM.parentFSM.changeState(StatsConst.EndState);
        }
    }

    public Direction GetAttackDirection(Vector2 from, Vector2 to)
    {
        var diff = to - from;
        if (diff.X >= 0)
        {
            return diff.Y>=0?Direction.SE:Direction.NE;
        }
        else
        {
            return diff.Y>=0?Direction.SW:Direction.NW;
        }
    }
}
