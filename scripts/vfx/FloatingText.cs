using Godot;
using System;

public partial class FloatingText : Label
{
    private Vector2 velocity = Vector2.Zero;
    private float gravity = 1000f;
    private bool active = false;
    private Random rng = new Random();

    private const float TWEEN_TIME = 0.3f;
    private const float AWAIT_TIME = 0.3f;
    
    public void Setup(Vector2 pos, string context, int fontSize, Color color)
    {
        Hide();
        Text = context;
        AddThemeColorOverride("font_color", color);
        AddThemeFontSizeOverride("font_size", fontSize);

        PivotOffset = Size / 2;
        Position = pos - Size / 2;
        
        var angleDeg = rng.Next(-60, 60) - 90; 
        var angleRad = Mathf.DegToRad(angleDeg);
        
        var speed = rng.Next(150,250);
        velocity = new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad))*speed;
        
        Show();
        active = true;
        GetTree().CreateTimer(AWAIT_TIME).Timeout += () =>
        {
            startFreeTween();
        };
    }

    public override void _Process(double delta)
    {
        if (!active)
            return;

        velocity.Y += gravity * (float)delta;
        Position += velocity * (float)delta;
    }

    private void startFreeTween()
    {
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(this, "modulate:a", 0.0f, TWEEN_TIME);
        tween.TweenProperty(this, "scale", new Vector2(0.5f, 0.5f), TWEEN_TIME);
        tween.Chain().TweenCallback(Callable.From(() => QueueFree()));
    }
}
