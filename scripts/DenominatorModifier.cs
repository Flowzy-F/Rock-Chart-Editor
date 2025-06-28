using Godot;
using System;

public partial class DenominatorModifier : Node
{
    LineEdit line_edit;
    public override void _Ready()
    {
        line_edit = this.GetChild<LineEdit>(1);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(line_edit);
        line_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(line_edit_focus_exited)));
        base._Ready();
    }
    void line_edit_focus_exited()
    {
        if (int.TryParse(line_edit.Text, out int value) && value > 0)
        {
            Editor.Denominator = value;
            Editor.Instance.GridDrawer.QueueRedraw();
            Editor.Instance.DrawerGroup.UpdateHeightOffset();
        }
        else
            line_edit.Text=Editor.Denominator.ToString();
    }
}
