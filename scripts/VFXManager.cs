using Godot;
using System;

public partial class VFXManager : Node
{
    private PackedScene floatingTextScene;

    public override void _Ready()
    {
        floatingTextScene = GD.Load<PackedScene>("res://scenes/floating_text.tscn");
        GlobalSignal.ShowFloatingTextEvent += onShowFloatingText;
        GlobalSignal.ShowFloatingHealTextEvent += onShowHealText;
    }

    public void SpawnFloatingText(Vector2 pos, string context, int fontSize, Color color)
    {
        var instance = floatingTextScene.Instantiate<FloatingText>();
        AddChild(instance);
        instance.Setup(pos, context, fontSize, color);
    }

    private void onShowFloatingText(Vector2 pos, int amount)
    {
        SpawnFloatingText(pos,$"-{amount}", 16, new Color(1,0,0,1));
    }
    
    private void onShowHealText(Vector2 pos, int amount)
    {
        SpawnFloatingText(pos,$"+{amount}", 16, new Color(0,1,0,1));
    }
}
