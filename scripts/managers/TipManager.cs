using Godot;
using System.Collections.Generic;

public partial class TipManager : Node
{
    static float fade_time = 0.1f;
     bool is_showing = false;
     Queue<TipInfo> tip_queue = new Queue<TipInfo>();
    public override void _Ready()
    {
    }
    public enum TipIcon
    {
        Information,
        Warning,
    }
    public enum TipColor
    {
        Red,
        Yellow,
        Green,
        Black,
    }
    public struct TipInfo
    {
        public string Text;
        public TipColor Color;
        public TipIcon Icon;
        public float Duration;
    }
    public void AddTip(string text, float duration, TipIcon icon, TipColor color)
    {
        TipInfo t = new TipInfo{Text=text,Icon=icon,Color=color,Duration=duration};
        tip_queue.Enqueue(t);
        try_show_next();
    }
    private void try_show_next()
    {
        if (!is_showing && tip_queue.Count > 0)
        {
            show_tip(tip_queue.Dequeue());
        }

    }
    private async void show_tip(TipInfo info)
    {
        if (fade_time * 2 > info.Duration) { fade_time = info.Duration / 2; }
        is_showing = true;
        Control tip = GD.Load<PackedScene>("res://scenes//tip.tscn").Instantiate<Control>();
        ColorRect bkg_rect = tip.GetChild<ColorRect>(0);
        ColorRect progress_rect = tip.GetChild<ColorRect>(1);
        Label text_label = tip.GetChild<Label>(2);
        Sprite2D information_sprite = tip.GetChild<Sprite2D>(3);
        Sprite2D warning_sprite = tip.GetChild<Sprite2D>(4);
        #region SetStyle
        text_label.Text = info.Text;
        switch (info.Color)
        {
            case TipColor.Black:
                bkg_rect.Color = new Color(0, 0, 0, 0.25f);
                progress_rect.Color = new Color(0, 0, 0, 0.3f);
                break;
            case TipColor.Yellow:
                bkg_rect.Color = new Color(255, 255, 0, 0.25f);
                progress_rect.Color = new Color(255, 255, 0, 0.3f);
                break;
            case TipColor.Red:
                bkg_rect.Color = new Color(255, 0, 0, 0.25f);
                progress_rect.Color = new Color(255, 0, 0, 0.3f);
                break;
            case TipColor.Green:
                bkg_rect.Color = new Color(0, 255, 0, 0.25f);
                progress_rect.Color = new Color(0, 255, 0, 0.3f);
                break;
            default:
                break;
        }
        switch (info.Icon)
        {
            case TipIcon.Information:
                information_sprite.Visible = true;
                warning_sprite.Visible = false;
                break;
            case TipIcon.Warning:
                information_sprite.Visible = false;
                warning_sprite.Visible = true;
                break;
            default: break;
        }
        #endregion
        tip.Modulate = new Color(tip.Modulate.R, tip.Modulate.G, tip.Modulate.B, 0);
        GetTree().Root.AddChild(tip);

        //float text_width = tip.GetChild<Label>(2).Size.X;
        //tip.GetChild<ColorRect>(0).Size = new Vector2(text_width+50, -1);
        //tip.GetChild<ColorRect>(1).Size = new Vector2(text_width+50, -1);

        var fadein_t = CreateTween();
        fadein_t.TweenProperty(tip, "modulate:a", 1.0f, fade_time);
        var t = CreateTween();
        t.TweenProperty(progress_rect, "scale:x", 0, info.Duration);

        await ToSignal(GetTree().CreateTimer(info.Duration-fade_time), Timer.SignalName.Timeout);
        var fadeout_t = CreateTween();
        fadeout_t.TweenProperty(tip, "modulate:a", 0, fade_time);
        await ToSignal(GetTree().CreateTimer(fade_time), Timer.SignalName.Timeout);
        tip.QueueFree();
        is_showing = false;
        try_show_next();
    }
    private static void pop_tip()
    {

    }
}
