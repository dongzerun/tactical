using Godot;
using System;
using System.Linq;

public partial class SwitchState : BaseState
{
    public override void OnEnter()
    {
        if (checkBattleFinished())
        {
            var factionName =battleNode.GetMainUnit().faction == Faction.Friendly? "friendly" : "enemy";
            GD.Print("Battle finished winner: " + factionName);
            return;
        }

        battleNode.allUnitsResource.SwitchToNext();
        parentFSM.changeState("StartState");
    }
    
    public override void OnExit()
    {
        GD.Print("Switch OnExit");
    }

    private bool checkBattleFinished()
    {
        var activeUnits = battleNode.allUnitsResource.GetAllUnits();
        
        // 过滤掉已经死亡的单位
        var aliveUnits = activeUnits.Where(u => u != null && IsInstanceValid(u) && !u.IsDead()).ToList();
        
        if (aliveUnits.Count <= 1)
        {
            GD.Print("Battle finished: too few units alive (" + aliveUnits.Count + ")");
            return true;
        }

        var fistFaction = aliveUnits[0].faction;
        for (int i = 1; i < aliveUnits.Count; i++)
        {
            if (aliveUnits[i].faction != fistFaction)
                return false;
        }
        
        GD.Print("Battle finished: all same faction");
        return true;
    }
}
