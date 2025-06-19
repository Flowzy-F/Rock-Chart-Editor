using Godot;
using System;

public partial class Serika : Sprite2D
{
    [Export] AudioStreamPlayer click_sound_player;
    bool rotated = false;
    Vector2 defaultScale;
    public override void _Ready()
    {
        defaultScale = this.Scale;
        base._Ready();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton e)
        {
            if (e.ButtonIndex == MouseButton.Left && e.Pressed && this.GetRect().HasPoint(ToLocal(e.Position)))
            {
                rotated = !rotated;
                Vector2 beginScale = rotated ? new Vector2(1.1f, 0.8f)*defaultScale
                    :new Vector2(-1.1f, 0.8f)*defaultScale;
                Vector2 destScale = rotated ? new Vector2(-defaultScale.X, defaultScale.Y) : defaultScale;
                Tween t = CreateTween();
                this.Scale = beginScale;
                t.SetTrans(Tween.TransitionType.Bounce);
                t.TweenProperty(this, "scale", destScale, 0.15f);
                click_sound_player.PitchScale = GD.Randf() / 2f + 0.75f;
                click_sound_player.Play();
            }
        }
        base._Input(@event);
    }
}
