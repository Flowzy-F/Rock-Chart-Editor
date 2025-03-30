using Godot;
using System;

public partial class AudioSettings : Node
{
    HScrollBar bgm_rate_scroll;
    LineEdit bgm_rate_edit;
    Button bgm_rate_reset_button;
    HScrollBar bgm_volume_scroll;
    LineEdit bgm_volume_edit;
    Button bgm_volume_reset_button;
    HScrollBar se_volume_scroll;
    LineEdit se_volume_edit;
    Button se_volume_reset_button;

    public override void _Ready()
    {
        bgm_rate_edit = this.GetChild(1).GetChild<LineEdit>(1);
        bgm_rate_scroll = this.GetChild(1).GetChild<HScrollBar>(2);
        bgm_rate_reset_button = this.GetChild(1).GetChild<Button>(3);
        bgm_volume_edit = this.GetChild(2).GetChild<LineEdit>(1);
        bgm_volume_scroll = this.GetChild(2).GetChild<HScrollBar>(2);
        bgm_volume_reset_button = this.GetChild(2).GetChild<Button>(3);
        se_volume_edit = this.GetChild(3).GetChild<LineEdit>(1);
        se_volume_scroll = this.GetChild(3).GetChild<HScrollBar>(2);
        se_volume_reset_button = this.GetChild(3).GetChild<Button>(3);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bgm_rate_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bgm_volume_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(se_volume_edit);

        bgm_rate_edit.Connect(LineEdit.SignalName.FocusExited,new Callable(this,nameof(bgm_rate_edit_focus_exited)));
        bgm_rate_scroll.Connect(HScrollBar.SignalName.ValueChanged,new Callable(this,nameof(bgm_rate_scroll_value_changed)));
        bgm_rate_reset_button.Connect(Button.SignalName.Pressed,new Callable(this,nameof(bgm_rate_reset_button_pressed)));
        bgm_volume_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(bgm_volume_edit_focus_exited)));
        bgm_volume_scroll.Connect(HScrollBar.SignalName.ValueChanged, new Callable(this, nameof(bgm_volume_scroll_value_changed)));
        bgm_volume_reset_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(bgm_volume_reset_button_pressed)));
        se_volume_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(se_volume_edit_focus_exited)));
        se_volume_scroll.Connect(HScrollBar.SignalName.ValueChanged, new Callable(this, nameof(se_volume_scroll_value_changed)));
        se_volume_reset_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(se_volume_reset_button_pressed)));
        base._Ready();
    }
    void bgm_rate_edit_focus_exited()
    {
        if (float.TryParse(bgm_rate_edit.Text, out float r) && r >= bgm_rate_scroll.MinValue && r <= bgm_rate_scroll.MaxValue)
        {
            bgm_rate_scroll.Value = r;
            Editor.Instance.BGMPlayer.PitchScale = r / 100;
        }
        else
            bgm_rate_edit.Text = bgm_rate_scroll.Value.ToString();
    }
    void bgm_rate_scroll_value_changed(double value)
    {
        Editor.Instance.BGMPlayer.PitchScale = (float)value / 100;
        bgm_rate_edit.Text = bgm_rate_scroll.Value.ToString();
    }
    void bgm_rate_reset_button_pressed()
    {
        bgm_rate_scroll.Value = 100;
        Editor.Instance.BGMPlayer.PitchScale = (float)bgm_rate_scroll.Value / 100;
        bgm_rate_edit.Text = bgm_rate_scroll.Value.ToString();
    }
    void bgm_volume_edit_focus_exited()
    {
        if (float.TryParse(bgm_volume_edit.Text, out float r) && r >= bgm_volume_scroll.MinValue && r <= bgm_volume_scroll.MaxValue)
        {
            bgm_volume_scroll.Value = r;
            Editor.Instance.BGMPlayer.VolumeLinear = r;
        }
        else
            bgm_volume_edit.Text = bgm_volume_scroll.Value.ToString();
    }
    void bgm_volume_scroll_value_changed(double value)
    {
        Editor.Instance.BGMPlayer.VolumeLinear = (float)value;
        bgm_volume_edit.Text = bgm_volume_scroll.Value.ToString();
    }
    void bgm_volume_reset_button_pressed()
    {
        bgm_volume_scroll.Value = 1;
        Editor.Instance.BGMPlayer.VolumeLinear = (float)bgm_volume_scroll.Value;
        bgm_volume_edit.Text = bgm_volume_scroll.Value.ToString();
    }
    void se_volume_edit_focus_exited()
    {
        if (float.TryParse(se_volume_edit.Text, out float r) && r >= se_volume_scroll.MinValue && r <= se_volume_scroll.MaxValue)
        {
            se_volume_scroll.Value = r;
            Editor.Instance.SEPlayerNormal.VolumeLinear = r;
            Editor.Instance.SEPlayerGold.VolumeLinear = r;
        }
        else
            se_volume_edit.Text = se_volume_scroll.Value.ToString();
    }
    void se_volume_scroll_value_changed(double value)
    {
        Editor.Instance.SEPlayerNormal.VolumeLinear = (float)value;
        Editor.Instance.SEPlayerGold.VolumeLinear = (float)value;
        se_volume_edit.Text = se_volume_scroll.Value.ToString();
    }
    void se_volume_reset_button_pressed()
    {
        se_volume_scroll.Value = 1;
        Editor.Instance.SEPlayerNormal.VolumeLinear = (float)se_volume_scroll.Value;
        Editor.Instance.SEPlayerGold.VolumeLinear = (float)se_volume_scroll.Value;
        se_volume_edit.Text = se_volume_scroll.Value.ToString();
    }
}
