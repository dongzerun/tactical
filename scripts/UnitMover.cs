using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class UnitMover : Node
{
    [Export] private GameArea gameArea;
    [Export] private float moveSpeed = 0.2f;

    public event Action MoveFinishedEvent;  // 保留 C# 事件用于静态订阅
    
    async public Task MoveUnit(Unit unit, List<Vector2I> path)
    {
        if(gameArea == null || gameArea.gameGrid == null)
            return;

        if (path.Count <= 1)
        {
            MoveFinishedEvent?.Invoke();
            return;
        }
        
        var startGridPos = path[0];
        var endGridPos = path.Last();
        
        var opsSuccess = gameArea.gameGrid.removeUnitInMap(startGridPos);
        GD.Print("removeUnitInMap " + startGridPos + " " + opsSuccess);
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetTrans(Tween.TransitionType.Linear);
        
        GD.Print("MoveUnit from "+startGridPos +  " to "+endGridPos);
        for (int i = 1; i < path.Count; i++)
        {
            var prevPos = path[i - 1];
            var currPos = path[i];
            var diff = currPos - prevPos;
            
            if (Unit.DIR_MAP.ContainsKey(diff))
            {
                var direction = Unit.DIR_MAP[diff];
                tween.TweenCallback(Callable.From(() => unit.PlayMove(direction)));
            }

            var targetPos = gameArea.getGlobalFromTile(currPos) + UnitSpawner.DEFAULT_OFFSET;
            tween.TweenProperty(unit, "position", targetPos, moveSpeed);
        }

        await ToSignal(tween, "finished");
        unit.PlayIdle();

        opsSuccess = gameArea.gameGrid.addUnitInMap(unit, endGridPos);
        GD.Print("addUnitInMap " + startGridPos + " " + opsSuccess);
        MoveFinishedEvent?.Invoke();
    } 
}
