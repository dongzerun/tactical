using Godot;
using System;
using System.Threading.Tasks;

public partial class AttackProcessor : Node
{
    [Export] public GameArea gameArea;
    [Export] public Battle battleNode;

    public async Task TriggerAttack(Unit attack, Unit defender)
    {
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
            // 断开事件连接
            attack.animatedSprite2DNode.AnimationFinished -= OnAttackAnimationFinished;
            tcs.SetResult(true);
        }
        
        await tcs.Task;
    }
    
    public Task ExecuteAttack(Unit attacker, Unit defender, float multiplier=1f)
    {
        var damage = CalculateDamage(attacker, defender) * multiplier;  
        GD.Print("Execute Attack " + damage);
        ApplyDamage(defender, (int)damage);
        return Task.CompletedTask;
    }

    public int ExecuteWorldDamage(Unit defender, int fixedWorldDamage)
    {
        var attackValue = Math.Max(0, fixedWorldDamage);
        var damage = CalculateDamageValue(attackValue, defender.GetDefense());
        ApplyDamage(defender, damage);
        return damage;
    }

    public int ExecuteHeal(Unit caster, Unit target, HealSkill skill)
    {
        var amount =  skill.HealAmount * skill.PowerMultiplier;  
        GD.Print("Execute Heal " + amount);
        target.Heal((int)amount);
        return (int)amount;
    }

    public void ApplyDamage(Unit target, int amount)
    {
        if (target == null)
            return;
        target.TakeDamage(Math.Max(0,amount));
    }

    public int CalculateDamage(Unit attacker, Unit defender)
    {
        if (attacker == null || defender == null)
            return 0;
        return CalculateDamageValue(attacker.GetAttackDamage(), defender.GetDefense());
    }

    public int CalculateDamageValue(int attackPower, int defensePower)
    {
        return Math.Max(1, attackPower-defensePower);
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
