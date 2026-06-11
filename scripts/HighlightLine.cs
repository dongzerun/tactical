using Godot;
using System;

public partial class HighlightLine : Line2D
{
    public void SetLineColor(Color color)
    {
        this.Modulate = color;
    }
}
