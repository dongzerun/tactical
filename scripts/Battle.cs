using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Battle : Node2D
{
    [Export] public PackedScene iconSkullScene;
    
    public UnitSpawner unitSpawner;
    public GameArea gameArea;
    public GridCalculator gridCalculator;
    public PathPainter pathPainter;
    public UnitMover mover;
    public RangeSelector rangeSelector;
    public BaseStateMachine baseStateMachine;
    public RangeCalculator rangeCalculator;
    public AttackProcessor attackProcessor;
    public KnockbackProcessor knockbackProcessor;
    
    public AllUnits allUnitsResource =  new AllUnits();
    public AllUnits backupAllUnitsResource = null;

    
    private IconSkull iconSkull;
    private BaseSkill currentSkill = null;

    public Dictionary<Vector2I, UnitInfo> unitPosDict = new Dictionary<Vector2I, UnitInfo>
    {
        {new Vector2I(0,0), UnitInfo.New(Faction.Friendly, "res://data/UnitStat.tres")},
        {new Vector2I(3,0), UnitInfo.New(Faction.Enemy, "res://data/UnitStat.tres")},
        {new Vector2I(3,-1), UnitInfo.New(Faction.Enemy, "res://data/UnitStat.tres")}
    };

    public override async void _Ready()
    {
        unitSpawner = GetNode<UnitSpawner>("UnitSpawner");
        gameArea = GetNode<GameArea>("GameArea");
        gridCalculator = GetNode<GridCalculator>("GridCalculator");
        pathPainter = GetNode<PathPainter>("PathPainter");
        mover = GetNode<UnitMover>("UnitMover");
        rangeSelector = GetNode<RangeSelector>("RangeSelector");
        baseStateMachine = GetNode<BaseStateMachine>("BattleStateMachine");
        rangeCalculator = GetNode<RangeCalculator>("RangeCalculator");
        attackProcessor = GetNode<AttackProcessor>("AttackProcessor");
        knockbackProcessor  = GetNode<KnockbackProcessor>("KnockbackProcessor");
        
        baseStateMachine.Initialize(this);
        await ToSignal(GetTree(), "process_frame");
        baseStateMachine.OnEnter();

        GlobalSignal.UnitDiedEvent += onUnitDied;
    }

    public override void _Process(double delta)
    {
        // run state machine
        baseStateMachine.StateProcess((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        // forwards input to state machine
        baseStateMachine.StateInput(@event);
    }

    public void UpdateAllUnitsBattleUnits()
    {
        allUnitsResource.UpdateBattleUnit(unit => GetUnitPosition(unit));
    }
    
    public BaseSkill GetSkill(int idx)
    {
        var unit = GetMainUnit();
        if (unit == null)
            return null;

        var selectSkill = unit.GetSkill(idx);
        if (selectSkill != null)
        {
            currentSkill = selectSkill;
            GD.Print("GetSkill " + idx + " name: " + selectSkill.SkillName);
        }

        return selectSkill;
    }

    public BaseSkill TrySelectSkillFromInput(InputEvent @event)
    {
        if (@event is not InputEventKey)
            return null;
        var keyEvent = @event as InputEventKey;
        if (!keyEvent.Pressed || keyEvent.Echo)
            return null;

        var skillIndex = 0;
        switch (keyEvent.Keycode)
        {
            case Key.Key1:
                skillIndex = 0;
                break;
            case Key.Kp1:
                skillIndex = 0;
                break;
            case Key.Key2:
                skillIndex = 1;
                break;
            case Key.Kp2:
                skillIndex = 1;
                break;
            case Key.Key3:
                skillIndex = 2;
                break;
            case Key.Kp3:
                skillIndex = 2;
                break;
            case Key.Key4:
                skillIndex = 3;
                break;
            case Key.Kp4:
                skillIndex = 3;
                break;
            case Key.Key5:
                skillIndex = 4;
                break;
            case Key.Kp5:
                skillIndex = 4;
                break;
            case Key.Key6:
                skillIndex = 5;
                break;
            case Key.Kp6:
                skillIndex = 5;
                break;
        }
        return GetSkill(skillIndex);
    }

    public void BackupAllUnits()
    {
        backupAllUnitsResource = allUnitsResource.Clone();
    }

    public void ClearBackup()
    {
        backupAllUnitsResource = null;
    }

    public bool RequestReset()
    {
        if (backupAllUnitsResource == null)
            return false;
        baseStateMachine.changeState("ResetState");
        return true;
    }

    public void ShowSkullOnUnit(Unit unit)
    {
        if (unit == null)
            return;

        if (iconSkull == null)
        {
            iconSkull = iconSkullScene.Instantiate<IconSkull>();
            AddChild(iconSkull);
        }
        
        HideSkull();

        var targetPos = unit.Position + new Vector2(0,-16);
        iconSkull.Position = targetPos;

        var faction = unit.faction;
        var targetColor = faction == Faction.Friendly? new Color(0f,1.0f,0f) :  new Color(1f,0f,0f);
        iconSkull.Show();
        iconSkull.TweenColor(targetColor);
    }

    public void HideSkull()
    {
        if (iconSkull == null)
            return;
        
        iconSkull.Hide();
        iconSkull.Modulate = new Color(1f,1f,1f,0f);
    }

    public Unit GetMainUnit()
    {
        return allUnitsResource.GetMainUnit();
    }

    private void onUnitDied(Unit unit)
    {
        var activeUnits = allUnitsResource.GetAllUnits();
        if (!activeUnits.Contains(unit))
            return;

        var unitIndex = activeUnits.IndexOf(unit);
        allUnitsResource.RemoveUnitAndUpdateIndex(unitIndex);
        
        var pos = GetUnitPosition(unit);
        var removed = gameArea.gameGrid.removeUnitInMap(pos);
        GD.Print("Unit " + unit.Name + " idx " + unitIndex + " pos " + pos + " died and remove from grid? " + removed);
        unit.QueueFree();
    }

    public BaseSkill GetCurrentSkill()
    {
        return currentSkill;
    }
    
    public Vector2I GetUnitPosition(Unit unit)
    {
        foreach (var kv in gameArea.gameGrid.GetGridDB())
        {
            if (kv.Value.unit == unit)
                return kv.Key;
        }
        return new Vector2I(-999,-999);
    }
    
    public GridData GetGridData(Vector2I cell)
    {
        return gameArea.gameGrid.GetGridData(cell);
    }
}
