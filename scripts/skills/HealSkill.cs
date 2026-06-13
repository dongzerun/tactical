using Godot;
using System;
using System.Threading.Tasks;

public partial class HealSkill : BaseSkill
{
    [ExportGroup("Skill Heal Config")]
    [Export] public int HealAmount = 10;
    
    public override Task applyEffect(Unit caster, Unit targetUnit, Vector2I targetPos, Battle battle)
    {
        GD.Print("HealSkill start to execute");
        battle.attackProcessor.ExecuteHeal(caster,targetUnit,this);
        return Task.CompletedTask;
    }
}
