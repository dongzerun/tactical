using Godot;
using System;
using System.Collections.Generic;
using Godot.NativeInterop;

public partial class PathPainter : Node
{
    [Export] private GameArea gameArea;

    private Dictionary<string, List<Line2D>> pathGroups=new();
    
    private Color defaultPainterColor = new Color(1f, 1f, 1f, 0.8f);
    public void ShowPath(List<Vector2I> cells, Color color, string groupName="default",float width=4.0f)
    {
        if (gameArea == null || cells.Count == 0)
            return;
        
        clearPath(groupName);
        //GD.Print("Going to paint Lines: " + cells.ToArray().Join(","));
        var line = new Line2D();
        line.Width = width;
        line.DefaultColor = color;

        List<Vector2> localPoints = new();
        foreach (var cell in cells)
        {
            localPoints.Add(gameArea.getGlobalFromTile(cell));
        }

        line.Points = localPoints.ToArray();
        line.Show();
        AddChild(line);
        pathGroups[groupName] = new List<Line2D>{line};
    }

    public void clearPath(string groupName)
    {
        if (groupName == "")
            clearAllPath();
        
        if (!pathGroups.ContainsKey(groupName))
            return;
        
        var path =  pathGroups[groupName];
        foreach (var line in path)
        {
            line.QueueFree();
        }
        pathGroups.Remove(groupName);
    }

    private void clearAllPath()
    {
        foreach (var lines in pathGroups.Values)
        {
            foreach (var l in lines)
            {
                l.QueueFree();
            }
        }
        pathGroups.Clear();
    }
}
