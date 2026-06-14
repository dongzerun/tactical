using Godot;
using System;
using System.Threading.Tasks;

public partial class KnockbackSkill : BaseSkill
{
    [Export] private int KnockbackDistance = 3;
    [Export] private float KnockbackPerCellTime = 0.1f;

    private TaskCompletionSource<bool> allEffectsCompletedSource = null;

    private int pendingEffectCount = 0;
    
    public override async Task applyEffect(Unit caster, Unit target, Vector2I targetPos, Battle battle)
    {
        if (battle == null)
            return;

        var kbp = battle.knockbackProcessor;
        if (kbp != null)
        {
            GD.Print("applyEffect call KnockbackUnit");
            var casterPos = battle.GetUnitPosition(caster);
            await kbp.KnockbackUnit(casterPos, target, KnockbackDistance, KnockbackPerCellTime);
        }
    }
    
    public override async Task Execute(Unit caster, Vector2I targetPos, Vector2I direction, RangeCalculator calculator, Battle battle)
    {
        if (battle == null)
            return;
        
        var effectCells = GetSkillAreaCells(targetPos, direction,battle.rangeCalculator);
        GD.Print("Knockback Skill target " + targetPos + " dir " + direction + " effect cells " + effectCells.ToArray().Join(" "));
        pendingEffectCount = 0;
        foreach (var cell in effectCells)
        {
            var cellData = battle.GetGridData(cell);
            if (cellData == null)
                continue;
            
            var targetUnit = cellData.unit as Unit;
            if (targetUnit != null && isValidTarget(caster, targetUnit))
            {
                pendingEffectCount++;
                GD.Print("Knockback skill call deferred applyEffectParallel");
                CallDeferred("applyEffectParallel", caster, targetUnit,targetPos,battle);
            }
        }

        if (pendingEffectCount > 0)
        {
            allEffectsCompletedSource = new TaskCompletionSource<bool>();
            await allEffectsCompletedSource.Task;
        }
    }

    public async void applyEffectParallel(Unit caster, Unit target, Vector2I targetPos, Battle battle)
    {
        GD.Print("applyEffectParallel targetPos " + targetPos);
        await applyEffect(caster, target, targetPos, battle);
        pendingEffectCount--;
        if (pendingEffectCount == 0 && allEffectsCompletedSource != null)
        {
            allEffectsCompletedSource.SetResult(true);
        }
    }
}
