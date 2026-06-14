using Godot;
using System;
using System.Threading.Tasks;

public partial class ExecuteSkill : BaseState
{
    public SkillState skillStateMachine;

    public override async void OnEnter()
    {
        GD.Print("ExecuteSkill onEnter");
        skillStateMachine = parentFSM as SkillState;
        var skill = skillStateMachine.GetCurrentSkill();
        var caster = battleNode.GetMainUnit();
        var targetPos = skillStateMachine.GetTargetPos();
        var direction = skill.IsDirection ? skillStateMachine.Direction : Vector2I.Zero;
        
        var unitDirection = toUnitDirection(direction, targetPos);
        await TriggerSkillAnimation(caster, skill.AnimationName, unitDirection);
        
        await skill.Execute(caster, targetPos,direction,battleNode.rangeCalculator, battleNode);
        parentFSM.parentFSM.parentFSM.changeState(StatsConst.EndState);
    }

    public override void OnExit()
    {
        GD.Print("ExecuteSkill onExit");
    }

    public async Task TriggerSkillAnimation(Unit attack, string animName, Direction direction)
    {
        attack.PlaySkillAnimation(animName, direction);
        
        var tcs = new TaskCompletionSource<bool>();
        attack.animatedSprite2DNode.AnimationFinished += OnAttackAnimationFinished;
        
        void OnAttackAnimationFinished()
        {
            attack.PlayIdle();
            // 断开事件连接
            attack.animatedSprite2DNode.AnimationFinished -= OnAttackAnimationFinished;
            tcs.SetResult(true);
        }
        
        await tcs.Task;
    }

    private Direction toUnitDirection(Vector2I dir, Vector2I targetPos)
    {
        var direction = dir;
        if (dir == Vector2I.Zero)
        {
            var caster = battleNode.GetMainUnit();
            var casterPos = battleNode.GetUnitPosition(caster);
            direction = Utils.CalculateDirection(casterPos, targetPos);
        }

        var normalized = new Vector2I(Math.Sign(direction.X),Math.Sign(direction.Y));
        var unit = battleNode.GetMainUnit();
        var fallback = unit!=null? unit.currentDirection : Direction.SE;
        if (Unit.DIR_MAP.ContainsKey(normalized))
        {
            return Unit.DIR_MAP[normalized];
        }
        return fallback;
    }
}
