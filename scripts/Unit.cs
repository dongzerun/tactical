using Godot;
using System;
using Godot.Collections;

public enum Direction
{
    NE,
    NW,
    SE,
    SW,
};

public enum Faction
{
    Enemy,
    Friendly,
    Neutral,
}

public class UnitInfo
{
    public Faction faction;
    public UnitStat UnitStat;

    public static UnitInfo New(Faction f, string unitStat)
    {
        var UnitInfo=new UnitInfo();
        UnitInfo.faction = f;
        UnitInfo.UnitStat = ResourceLoader.Load<UnitStat>(unitStat);
        return UnitInfo;
    }
}

public partial class Unit : Node2D
{
    [Export] public AnimatedSprite2D animatedSprite2DNode;
    [Export] public Faction faction;
    [Export] public UnitStat unitStat
    {
        get => _unitStat;
        set => _unitStat = value;
    }
    private UnitStat _unitStat;
    [Export] public Array<BaseSkill> skills;

    public event Action attackTriggerDamage;

    public int currentHP = 1;
    private bool isDying = false;
    
    public static readonly System.Collections.Generic.Dictionary<Vector2I, Direction> DIR_MAP = new System.Collections.Generic.Dictionary<Vector2I, Direction>
    {
        { new Vector2I(0, -1), Direction.NE },
        { new Vector2I(-1, 0), Direction.NW },
        { new Vector2I(1, 0), Direction.SE },
        { new Vector2I(0, 1), Direction.SW }
    };
    
    private const string ANIM_IDLE = "IDLE";
    private const string ANIM_RUN = "RUN";
    private const string ANIM_DEATH = "DEATH";
    private const string ANIM_ATTACK = "ATTACK";
    private const string ANIM_SKILL = "SKILL";

    public Direction currentDirection = Direction.SE;
    
    public override async void _Ready()
    {
        animatedSprite2DNode = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        currentHP = unitStat.GetMaxHP();
    }

    private bool attackDamageTriggered = false;
    
    public override void _Process(double delta)
    {
        if (animatedSprite2DNode == null || !IsInstanceValid(animatedSprite2DNode))
            return;
            
        var currentAnim = animatedSprite2DNode.Animation;
        // We only trigger damage once when play attack animation progress is 90%
        if (currentAnim != null && ((string)currentAnim).EndsWith(ANIM_ATTACK))
        {
            var totalFrames = animatedSprite2DNode.SpriteFrames.GetFrameCount(currentAnim);
            var currentFrame = animatedSprite2DNode.Frame;
            var progress = (float)currentFrame / totalFrames;
            
            if (progress >= 0.9f && !attackDamageTriggered)
            {
                GD.Print($"Attack animation progress: {progress * 100}%");
                attackDamageTriggered = true;
                animationAttackTriggerDamage();
            }
        }
        else
        {
            attackDamageTriggered = false;
        }
    }

    private void playAnimation(string state, Direction dir=Direction.SE)
    {
        currentDirection = dir;
        var animName = $"{dir.ToString()}_{state}";
        if (animatedSprite2DNode == null || !animatedSprite2DNode.SpriteFrames.HasAnimation(animName))
        {
            GD.Print("animated null or anim name " + animName + " not exists");
            return;
        }

        animatedSprite2DNode.Animation = $"{dir.ToString()}_{state}";
        animatedSprite2DNode.Play();
    }
    
    public void PlaySkillAnimation(string state, Direction dir=Direction.SE)
    {
        currentDirection = dir;
        var animName = $"{dir.ToString()}_{state}";
        if (animatedSprite2DNode == null || !animatedSprite2DNode.SpriteFrames.HasAnimation(animName))
        {
            GD.Print("animated null or anim name " + animName + " not exists");
            return;
        }

        animatedSprite2DNode.Animation = $"{dir.ToString()}_{state}";
        animatedSprite2DNode.Play();
    }

    public void PlayMove(Direction direction)
    {
        playAnimation(ANIM_RUN, direction);
    }

    public void PlayIdle()
    {
        playAnimation(ANIM_IDLE, currentDirection);
    }
    
    public void PlayAttack(Direction dir)
    {
        playAnimation(ANIM_ATTACK, dir);
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
    
    public void PlayDeath()
    {
        playAnimation(ANIM_DEATH, currentDirection);
    }
    
    public void PlaySkill()
    {
        playAnimation(ANIM_SKILL, currentDirection);
    }

    public BaseSkill GetSkill(int idx)
    {
        if (idx < 0 || idx >= skills.Count)
            return null;
        return skills[idx];
    }

    public int GetMovePoints()
    {
        return unitStat.GetMovePoint();
    }

    public bool IsDead()
    {
        return currentHP <= 0 || isDying;
    }

    public void animationAttackTriggerDamage()
    {
        attackTriggerDamage?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        if (isDying)
            return;
        
        currentHP -= damage;
        GlobalSignal.RaiseShowFloatingTextEvent(Position, damage);
        if (currentHP <= 0)
        {
            GD.Print("currentHP < 0, unit died now");
            die();
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0)
            return;

        currentHP = Math.Min(currentHP+amount, unitStat.GetMaxHP());
        GlobalSignal.RaiseShowFloatingHealTextEvent(Position, amount);
    }

    public void die()
    {
        if (isDying)
            return;
        isDying = true;
        
        if (animatedSprite2DNode == null || !IsInstanceValid(animatedSprite2DNode))
        {
            GD.Print("animatedSprite2DNode is null or invalid, skip death animation");
            GlobalSignal.RaiseUnitDied(this);
            return;
        }
        
        playAnimation(ANIM_DEATH, currentDirection);
        // 使用一次性事件连接
        animatedSprite2DNode.AnimationFinished += OnDiedAnimationFinished;
    }

    private void OnDiedAnimationFinished()
    {
        // 断开事件连接，防止重复触发
        if (animatedSprite2DNode != null && IsInstanceValid(animatedSprite2DNode))
        {
            animatedSprite2DNode.AnimationFinished -= OnDiedAnimationFinished;
        }
        GlobalSignal.RaiseUnitDied(this);
    }
    

    public void setUnitColor(Color color)
    {
        if (animatedSprite2DNode != null)
            animatedSprite2DNode.Modulate = color;
    }
    
    public int GetAttackDamage()
    {
        return unitStat.GetAttackDamage();
    }
    
    public int GetDefense()
    {
        return unitStat.GetDefense();
    }
    
    public int GetAttackRange()
    {
        return unitStat.GetAttackRange();
    }

    public int GetMoveCost(Terrain terrain)
    {
        if (UnitStat.moveCostMap.ContainsKey(terrain))
            return UnitStat.moveCostMap[terrain];
        return -1;
    }

    public int GetMaxHP()
    {
        return unitStat.GetMaxHP();
    }

    public BattleUnit CreateBattleUnit(Vector2I cellPos)
    {
        BattleUnit bUnit = new BattleUnit();
        bUnit.SetData(unitStat,faction,cellPos,currentDirection,currentHP);
        return bUnit;
    }

    public void RestoreFromBattleUnit(BattleUnit bUnit)
    {
        unitStat = bUnit.unitStat;
        faction = bUnit.faction;
        currentHP = bUnit.currentHP;
        currentDirection =  bUnit.direction;
        PlayIdle();
    }

    public void updateVisual()
    {
        if (!IsInsideTree())
            return;

        switch (faction)
        {
            case Faction.Enemy:
                setUnitColor(new Color(0.5f, 1.0f,0.5f));
                break;
            case Faction.Friendly:
                setUnitColor(new Color(1.0f,0.5f,0.5f));
                break;
        }
    }
}
