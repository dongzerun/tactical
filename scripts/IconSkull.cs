using Godot;
using System;

public partial class IconSkull : Sprite2D
{
    private Tween tween;

    public void TweenColor(Color target, float duration = 0.3f)
    {
        tween?.Kill();
        tween = CreateTween();
        tween.TweenProperty(this, "modulate", target, duration);
    }
}
