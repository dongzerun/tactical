using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RangeSelector : Node
{
    [Export] private GameArea gameArea;
    [Export] private PackedScene highlightAreaScene;

    private Dictionary<string, int> PriorityMap = new Dictionary<string, int>
    {
        {"move_Range",0},
        {"attackRange",1},
        {"default", 1}
    };
    private Dictionary<string,List<Node2D>> _groups = new();

    public void ShowRange(List<Vector2I> cells, Color color, string groupName="default")
    {
        ClearRange(groupName);
        if (gameArea == null)
            return;

        List<Node2D> group = new();
        _groups[groupName] = group;
        foreach (var cell in cells)
        {
            var highlightArea = highlightAreaScene.Instantiate<HighlightArea>();
            highlightArea.Position = gameArea.getGlobalFromTile(cell);
            highlightArea.SetColor(color);
            highlightArea.Show();
            AddChild(highlightArea);
            group.Add(highlightArea);
        }

        ReorderHighlights();
    }

    public void ReorderHighlights()
    {
        var activeGroups = _groups.Keys.ToList();
        //if (activeGroups.Count <= 1)
          //  return;

        // 按优先级排序组
        var sortedGroups = activeGroups.OrderBy(s => PriorityMap.ContainsKey(s) ? PriorityMap[s] : 0).ToList();

        GD.Print("ReorderHighlights " + activeGroups.Count + " groups " + sortedGroups.ToArray().Join(" "));
        // 重新排序节点在场景树中的顺序
        foreach (var groupName in sortedGroups)
        {
            if (!_groups.ContainsKey(groupName))
                continue;

            var group = _groups[groupName];
            foreach (var node in group)
            {
                if (node != null && IsInstanceValid(node))
                {
                    MoveChild(node, -1);
                }
            }
        }
    }

    public void ClearRange(string groupName)
    {
        if (groupName == "")
            ClearAllRanges();

        if (!_groups.ContainsKey(groupName))
            return;
        
        var group=_groups[groupName];
        foreach (var n in group)
        {
            n.QueueFree();
        }
        _groups[groupName].Clear();
    }

    public void ClearAllRanges()
    {
        foreach (var nodes in _groups.Values)
        {
            foreach (var n in nodes)
            {
                n.QueueFree();
            }
            nodes.Clear();
        }
    }
}
