using Godot;
using System;

public partial class Serika : Sprite2D
{
    [Export] AudioStreamPlayer click_sound_player;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton e)
        {
            if (e.ButtonIndex == MouseButton.Left && e.Pressed && this.GetRect().HasPoint(ToLocal(e.Position)))
            {
                Tween t = CreateTween();
                this.Scale = new Vector2(0.7f, 0.4f);
                t.SetTrans(Tween.TransitionType.Bounce);
                t.TweenProperty(this, "scale", new Vector2(0.6f, 0.6f), 0.15f);
                click_sound_player.PitchScale = GD.Randf() / 2f + 0.75f;
                click_sound_player.Play();
            }
        }
        base._Input(@event);
    }
}
