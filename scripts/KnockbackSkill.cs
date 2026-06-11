using Godot;
using System;
using System.Threading.Tasks;

public partial class KnockbackSkill : BaseSkill
{
    [Export] private int KnockbackDistance = 3;
    [Export] private float KnockbackPerCellTime = 0.1f;

    [Signal] public delegate void AllEffectsCompletedEventHandler();

    private int pendingEffectCount = 0;
    
    public override async Task applyEffect(Unit caster, Unit target, Vector2I targetPos, Battle battle)
    {
        if (battle == null)
            return;

        var kbp = battle.knockbackProcessor;
        if (kbp != null)
        {
            await kbp.KnockbackUnit(targetPos, target, KnockbackDistance, KnockbackPerCellTime);
        }
    }
    
    public override async Task Execute(Unit caster, Vector2I targetPos, Vector2I direction, RangeCalculator calculator, Battle battle)
    {
        if (battle == null)
            return;
        
        var effectCells = GetSkillAreaCells(targetPos, direction,battle.rangeCalculator);
        pendingEffectCount = 0;
        foreach (var cell in effectCells)
        {
            var cellData = battle.gameArea.gameGrid.gridDB[cell];
            var targetUnit = cellData.unit as Unit;
            if (targetUnit != null && isValidTarget(caster, targetUnit))
            {
                pendingEffectCount++;
                CallDeferred("applyEffectParallel", caster, targetUnit,targetPos,battle);
            }
        }

        if (pendingEffectCount >0)
        {
            await ToSignal(this, nameof(AllEffectsCompletedEventHandler));
        }
    }

    public async void applyEffectParallel(Unit caster, Unit target, Vector2I targetPos, Battle battle)
    {
        await applyEffect(caster, target, targetPos, battle);
        pendingEffectCount--;
        if (pendingEffectCount == 0)
        {
            EmitSignal(nameof(AllEffectsCompletedEventHandler));
        }
    }
}
