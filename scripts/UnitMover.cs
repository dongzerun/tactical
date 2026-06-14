using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class UnitMover : Node
{
    [Export] private GameArea _gameArea;
    [Export] private float _moveSpeed = 0.2f;

    public event Action MoveFinishedEvent;
    
    public async Task MoveUnit(Unit unit, List<Vector2I> path)
    {
        if(_gameArea == null || _gameArea.gameGrid == null)
            return;

        if (path.Count <= 1)
        {
            MoveFinishedEvent?.Invoke();
            return;
        }
        
        var startGridPos = path[0];
        var endGridPos = path.Last();
        
        var opsSuccess = _gameArea.gameGrid.removeUnitInMap(startGridPos);
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetTrans(Tween.TransitionType.Linear);
        
        GD.Print("MoveUnit from "+startGridPos +  " to "+endGridPos);
        for (int i = 1; i < path.Count; i++)
        {
            var prevPos = path[i - 1];
            var currPos = path[i];
            var diff = currPos - prevPos;
            
            if (Consts.DIR_MAP.ContainsKey(diff))
            {
                var direction = Consts.DIR_MAP[diff];
                tween.TweenCallback(Callable.From(() => unit.PlayMove(direction)));
            }

            var targetPos = _gameArea.getGlobalFromTile(currPos) + UnitSpawner.DEFAULT_OFFSET;
            tween.TweenProperty(unit, "position", targetPos, _moveSpeed);
        }

        await ToSignal(tween, "finished");
        unit.PlayIdle();

        opsSuccess = _gameArea.gameGrid.addUnitInMap(unit, endGridPos);
        MoveFinishedEvent?.Invoke();
    } 
}
