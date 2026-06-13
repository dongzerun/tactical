using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

public enum OriginType
{
    SELF, // skill original start pos is self
    GLOBAL, // skill original start global
    RANGE // range selector a area
}

public enum TargetFilter
{
    ALL, // all units, include all factions
    ENEMY_ONLY, // only targeting to enemy faction
    FRIEND_ONLY, // only targeting to enemy faction
    SELF_ONLY // only targeting to self
}

public partial class BaseSkill : Resource
{
    [ExportGroup("Base Skill Info")] 
    [Export] public string SkillName = "Unknown";
    [Export] public string AnimationName;
    [Export] public string Descriptions;
    
    [ExportGroup("Skill Casting Info")]
    [Export] public OriginType originType =  OriginType.RANGE;
    [Export] public int castRange = 1;
    [Export] public DistanceAlgorithm castAlgorithm = DistanceAlgorithm.MANHATTAN;
    
    [ExportGroup("Skill Effect Area")]
    [Export] public Vector2I AreaRange = Vector2I.Zero;
    [Export] public ShapeType AreaShape = ShapeType.CIRCLE;
    [Export] public DistanceAlgorithm AreaAlgorithm = DistanceAlgorithm.MANHATTAN;
    [Export] public bool IsDirection = false;

    [ExportGroup("Skill Effect Config")] 
    [Export] public float PowerMultiplier = 1f;
    [Export] public bool IncludeSelf = false;
    [Export] public TargetFilter Filter = TargetFilter.ENEMY_ONLY;

    public virtual async Task Execute(Unit caster, Vector2I targetPos, Vector2I direction, RangeCalculator calculator, Battle battle)
    {
        GD.Print("GOing to execute skill: " + SkillName);
        var affectedCells=GetSkillAreaCells(targetPos, direction, calculator);
        foreach (var cell in affectedCells)
        {
            var cellData = battle.gameArea.gameGrid.gridDB[cell];
            var targetUnit = cellData.unit as Unit;
            if (targetUnit != null && isValidTarget(caster, targetUnit))
            {
                GD.Print("GOing to execute skill: " + SkillName + " applyEffect target pos " + targetPos);
                await applyEffect(caster, targetUnit, targetPos,battle);
            }
            else
            {
                GD.Print("Ignore to execute skill: " + SkillName + " target pos " + targetPos);
            }
        }
    }
    
    public virtual async Task applyEffect(Unit caster, Unit targetUnit, Vector2I targetPos, Battle battle)
    {
        if (battle.attackProcessor != null)
        {
            battle.attackProcessor.ExecuteAttack(targetUnit, targetUnit,PowerMultiplier);
        }
    }
    
    public List<Vector2I> GetSkillAreaCells(Vector2I targetPos, Vector2I direction, RangeCalculator calculator)
    {
        List<Vector2I> results = new();

        if (IsDirection)
        {
            results = calculator.GetDirectionalRangeCells(targetPos, direction, AreaRange, AreaShape);
        }
        else
        {
            results = calculator.GetRangeCells(targetPos, AreaRange.X, AreaAlgorithm);
        }
        return results;
    }

    public bool isValidTarget(Unit caster, Unit targetUnit)
    {
        if (targetUnit == null || caster == null)
            return false;

        if (targetUnit == caster)
        {
            if (Filter == TargetFilter.SELF_ONLY)
                return true;
            return IncludeSelf == true;
        }

        switch (Filter)
        {
            case TargetFilter.ALL:
                return true;
            case TargetFilter.SELF_ONLY:
                return targetUnit == caster;
            case TargetFilter.ENEMY_ONLY:
                return targetUnit.faction != caster.faction;
            case TargetFilter.FRIEND_ONLY:
                return targetUnit.faction == caster.faction;
        }

        return false;
    }

    public List<Vector2I> GetCastRangeCells(Unit caster, GameGrid gameGrid, RangeCalculator rangeCalculator)
    {
        List<Vector2I> results = new();
        if (caster == null || gameGrid == null)
            return results;

        var casterPos = gameGrid.getUnitPosition(caster);
        switch (originType)
        {
            case OriginType.RANGE:
                if (rangeCalculator != null)
                {
                    return rangeCalculator.GetRangeCells(casterPos, castRange, castAlgorithm);
                }

                break;
            case OriginType.GLOBAL:
                foreach (var pos in gameGrid.gridDB.Keys)
                {
                    if (pos is Vector2I)
                    {
                        results.Add(pos);
                    }
                }
                break;
            case OriginType.SELF:
                results.Add(casterPos);
                break;
        }

        return results;
    }
}
