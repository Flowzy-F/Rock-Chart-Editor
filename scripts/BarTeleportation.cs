using Godot;
using System;

public partial class BarTeleportation : Node
{
    LineEdit line_edit;
    Button teleport_button;
    public override void _Ready()
    {
        line_edit = this.GetChild<LineEdit>(1);
        teleport_button = this.GetChild<Button>(2);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(line_edit);

        teleport_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(on_teleport_button_pressed)));
        base._Ready();
    }

    void on_teleport_button_pressed()
    {
        if (MediaManager.IsPlaying) return;
        if (int.TryParse(line_edit.Text, out int value) && value >= Math.Ceiling(Editor.BottomPosition))
        {
            Editor.Bar = value;
            Editor.Instance.DrawerGroup.RedrawAll();
        }
        else
        {
            //TODO
        }
    }
}
