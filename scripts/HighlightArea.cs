using Godot;
using System;

public partial class HighlightArea : Polygon2D
{
    [Export] private HighlightLine highlightLine;

    public void SetAreaColor(Color color)
    {
        this.Color = color;
    }

    public void SetOutlineColor(Color color)
    {
        highlightLine.SetLineColor(color);
    }
}
