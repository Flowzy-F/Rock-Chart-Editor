using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class CameraNoteEdit : PanelContainer
{
    LineEdit bar_edit_begin;
    LineEdit fraction_edit_begin;
    LineEdit bar_edit_end;
    LineEdit fraction_edit_end;
    LineEdit duration_bar_edit;
    LineEdit duration_fraction_edit;
    OptionButton easing_option;
    LineEdit pos_move_edit_X;
    LineEdit pos_move_edit_Y;
    LineEdit pos_move_edit_Z;
    LineEdit rot_move_edit_X;
    LineEdit rot_move_edit_Y;
    LineEdit rot_move_edit_Z;
    CurveDrawer curveDrawer;
    List<LineEdit> edits = new List<LineEdit>();
    List<CameraNoteData> pointing_notedatas = new List<CameraNoteData>();
    public override void _Ready()
    {
        bar_edit_begin = this.GetChild(0).GetChild(1).GetChild<LineEdit>(1);
        fraction_edit_begin = this.GetChild(0).GetChild(1).GetChild<LineEdit>(2);
        bar_edit_end = this.GetChild(0).GetChild(2).GetChild<LineEdit>(1);
        fraction_edit_end = this.GetChild(0).GetChild(2).GetChild<LineEdit>(2);

        duration_bar_edit = this.GetChild(0).GetChild(3).GetChild<LineEdit>(1);
        duration_fraction_edit = this.GetChild(0).GetChild(3).GetChild<LineEdit>(2);

        easing_option = this.GetChild(0).GetChild(4).GetChild<OptionButton>(1);
        curveDrawer = this.GetChild(0).GetChild(4).GetChild<CurveDrawer>(2);

        pos_move_edit_X = this.GetChild(0).GetChild(5).GetChild<LineEdit>(1);
        pos_move_edit_Y = this.GetChild(0).GetChild(5).GetChild<LineEdit>(2);
        pos_move_edit_Z = this.GetChild(0).GetChild(5).GetChild<LineEdit>(3);

        rot_move_edit_X = this.GetChild(0).GetChild(6).GetChild<LineEdit>(1);
        rot_move_edit_Y = this.GetChild(0).GetChild(6).GetChild<LineEdit>(2);
        rot_move_edit_Z = this.GetChild(0).GetChild(6).GetChild<LineEdit>(3);
        #region AddChangeNeedingEdits & Add Controls

        edits.Add(bar_edit_begin);
        edits.Add(fraction_edit_begin);
        edits.Add(bar_edit_end);
        edits.Add(fraction_edit_end);
        edits.Add(duration_bar_edit);
        edits.Add(duration_fraction_edit);
        edits.Add(pos_move_edit_X);
        edits.Add(pos_move_edit_Y);
        edits.Add(pos_move_edit_Z);
        edits.Add(rot_move_edit_X);
        edits.Add(rot_move_edit_Y);
        edits.Add(rot_move_edit_Z);
        foreach (var e in edits) Editor.Instance.FocusChangeNeedingLineEdits.Add(e);
        #endregion
        #region Register easing
        easing_option.Clear();
        easing_option.AddItem("Linear", 0);
        easing_option.AddItem("InSine", 1);
        easing_option.AddItem("OutSine", 2);
        easing_option.AddItem("InOutSine", 3);
        easing_option.AddItem("InQuad", 4);
        easing_option.AddItem("OutQuad", 5);
        easing_option.AddItem("InOutQuad", 6);
        easing_option.AddItem("InCubic", 7);
        easing_option.AddItem("OutCubic", 8);
        easing_option.AddItem("InOutCubic", 9);
        easing_option.AddItem("InQuart", 10);
        easing_option.AddItem("OutQuart", 11);
        easing_option.AddItem("InOutQuart", 12);
        easing_option.AddItem("InQuint", 13);
        easing_option.AddItem("OutQuint", 14);
        easing_option.AddItem("InOutQuint", 15);
        easing_option.AddItem("InExpo", 16);
        easing_option.AddItem("OutExpo", 17);
        easing_option.AddItem("InOutExpo", 18);
        easing_option.AddItem("InCirc", 19);
        easing_option.AddItem("OutCirc", 20);
        easing_option.AddItem("InOutCirc", 21);
        easing_option.AddItem("InElastic", 22);
        easing_option.AddItem("OutElastic", 23);
        easing_option.AddItem("InOutElastic", 24);
        easing_option.AddItem("InBack", 25);
        easing_option.AddItem("OutBack", 26);
        easing_option.AddItem("InOutBack", 27);
        easing_option.AddItem("InBounce", 28);
        easing_option.AddItem("OutBounce", 29);
        easing_option.AddItem("InOutBounce ", 30);
        easing_option.AddItem("Flash", 31);
        easing_option.AddItem("InFlash", 32);
        easing_option.AddItem("OutFlash", 33);
        easing_option.AddItem("InOutFlash", 34);
        easing_option.Select(0);
        #endregion
        //Connection
        bar_edit_begin.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bar_begin)));
        fraction_edit_begin.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_fraction_begin)));
        bar_edit_end.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bar_end)));
        fraction_edit_end.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_fraction_end)));
        duration_bar_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_duration_bar)));
        duration_fraction_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_duration_fraction)));
        easing_option.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(change_easing)));
        pos_move_edit_X.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_pos_x)));
        pos_move_edit_Y.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_pos_y)));
        pos_move_edit_Z.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_pos_z)));
        rot_move_edit_X.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_rot_x)));
        rot_move_edit_Y.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_rot_y)));
        rot_move_edit_Z.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_rot_z)));
        base._Ready();
    }
    public void UpdateLineEdits()
    {
        if (SelectedNoteList.Count <= 0) return;
        pointing_notedatas.Clear();
        foreach (NoteHash h in SelectedNoteList)
        {
            if (NoteMap.TryGetValue(h, out var d))
            {
                if (d.Type != NoteType.Camera)
                    throw new Exception("CameraNoteEdit:Poiting at a wrong sort of note!");//TODO
                pointing_notedatas.Add((CameraNoteData)d);
            }
            else
                throw new Exception("NoteEdit:Pointing Note not founded in NoteMap!");
        }
        //Change Text
        bar_edit_begin.Text = pointing_notedatas.All(n => n.Position.GetWhole() == pointing_notedatas[0].Position.GetWhole())
            ? pointing_notedatas[0].Position.GetWhole().ToString() : "...";
        fraction_edit_begin.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";

        Utils.Fraction end = pointing_notedatas[0].Position + pointing_notedatas[0].Duration;
        bar_edit_end.Text = end.GetWhole().ToString();
        fraction_edit_end.Text = $"{end.GetTrueNumerator()}/{end.Denominator}";

        duration_bar_edit.Text = pointing_notedatas[0].Duration.GetWhole().ToString();
        duration_fraction_edit.Text = $"{pointing_notedatas[0].Duration.GetTrueNumerator()}/{pointing_notedatas[0].Duration.Denominator}";

        easing_option.Selected = (int)pointing_notedatas[0].Easing;
        curveDrawer.easingType = pointing_notedatas[0].Easing;
        curveDrawer.QueueRedraw();

        pos_move_edit_X.Text = pointing_notedatas[0].PositionMovementX.ToString();
        pos_move_edit_Y.Text = pointing_notedatas[0].PositionMovementY.ToString();
        pos_move_edit_Z.Text = pointing_notedatas[0].PositionMovementZ.ToString();
        rot_move_edit_X.Text = pointing_notedatas[0].RotationMovementX.ToString();
        rot_move_edit_Y.Text = pointing_notedatas[0].RotationMovementY.ToString();
        rot_move_edit_Z.Text = pointing_notedatas[0].RotationMovementZ.ToString();

        //Only editable when select a single note.
        if (pointing_notedatas.Count == 1)
        {
            foreach (var e in edits) e.Editable = true;
            easing_option.Disabled = false;
        }
        else
        {
            foreach (var e in edits) e.Editable = false;
            easing_option.Disabled = true;
        }
    }
    void change_bar_begin()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(bar_edit_begin.Text.Trim(), out int bar_r) && bar_r >= 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetTrueNumerator() + bar_r * SelectedNoteList[0].Position.Denominator
                , SelectedNoteList[0].Position.Denominator), NoteType.Camera)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        bar_edit_begin.Text = pointing_notedatas[0].Position.GetWhole().ToString();
    }
    void change_fraction_begin()//Only allowed when selecting a single note.
    {
        string[] info = fraction_edit_begin.Text.Trim().Split('/');
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (info.Length >= 2 && int.TryParse(info[0], out int num_r) && num_r >= 0 && int.TryParse(info[1], out int den_r) &&
            (num_r >= 0 && num_r < den_r && den_r > 0))
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole() * den_r + num_r, den_r), NoteType.Camera)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (info.Length == 1 && int.TryParse(info[0], out int r) && r == 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole(), 1), NoteType.Camera)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        fraction_edit_begin.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
    }
    void change_bar_end()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(bar_edit_end.Text.Trim(), out int bar_r) && bar_r >= 0)
        {
            string[] end_fraction = fraction_edit_end.Text.Trim().Split('/');
            int.TryParse(end_fraction[0]!, out int num);
            int.TryParse(end_fraction[0]!, out int den);
            Utils.Fraction target_end = d.Position + new Utils.Fraction(num, den) + bar_r;
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration, target_end - d.Position));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }

        Utils.Fraction end = pointing_notedatas[0].Position + pointing_notedatas[0].Duration;
        bar_edit_end.Text = end.GetWhole().ToString();
        fraction_edit_end.Text = $"{end.GetTrueNumerator()}/{end.Denominator}";

        duration_bar_edit.Text = pointing_notedatas[0].Duration.GetWhole().ToString();
        duration_fraction_edit.Text = $"{pointing_notedatas[0].Duration.GetTrueNumerator()}/{pointing_notedatas[0].Duration.Denominator}";
    }
    void change_fraction_end()//Only allowed when selecting a single note.
    {
        string[] info = fraction_edit_end.Text.Trim().Split('/');
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (info.Length >= 2 && int.TryParse(info[0], out int num_r) && num_r >= 0 && int.TryParse(info[1], out int den_r) &&
            (num_r >= 0 && num_r < den_r && den_r > 0))
        {
            int.TryParse(bar_edit_end.Text.Trim(), out int bar_r);
            Utils.Fraction target_end = d.Position + new Utils.Fraction(num_r, den_r) + bar_r;
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration, target_end - d.Position));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (info.Length == 1 && int.TryParse(info[0], out int r) && r == 0)
        {
            int.TryParse(bar_edit_end.Text.Trim(), out int bar_r);
            Utils.Fraction target_end = d.Position + bar_r;
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration, target_end - d.Position));
        }
        Utils.Fraction end = pointing_notedatas[0].Position + pointing_notedatas[0].Duration;
        bar_edit_end.Text = end.GetWhole().ToString();
        fraction_edit_end.Text = $"{end.GetTrueNumerator()}/{end.Denominator}";

        duration_bar_edit.Text = pointing_notedatas[0].Duration.GetWhole().ToString();
        duration_fraction_edit.Text = $"{pointing_notedatas[0].Duration.GetTrueNumerator()}/{pointing_notedatas[0].Duration.Denominator}";
    }
    void change_easing(int selected)//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
            .SetFor<CameraNoteData, EasingType>(data => data.Easing, (EasingType)selected));
        Editor.Instance.NoteDrawer.QueueRedraw();

        curveDrawer.easingType = (EasingType)selected;
        curveDrawer.QueueRedraw();
    }
    void change_pos_x()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(pos_move_edit_X.Text, out int x_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.PositionMovementX, x_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        pos_move_edit_X.Text = x_r.ToString();
    }
    void change_pos_y()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(pos_move_edit_Y.Text, out int y_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.PositionMovementY, y_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        pos_move_edit_Y.Text = y_r.ToString();
    }
    void change_pos_z()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(pos_move_edit_Z.Text, out int z_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.PositionMovementZ, z_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        pos_move_edit_Z.Text = z_r.ToString();
    }
    void change_rot_x()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(rot_move_edit_X.Text, out int x_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.RotationMovementX, x_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        rot_move_edit_X.Text = x_r.ToString();
    }
    void change_rot_y()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(rot_move_edit_Y.Text, out int y_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.RotationMovementY, y_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        rot_move_edit_Y.Text = y_r.ToString();
    }
    void change_rot_z()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(rot_move_edit_Z.Text, out int z_r))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, float>(data => data.RotationMovementZ, z_r));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        rot_move_edit_Z.Text = z_r.ToString();
    }
    void change_duration_bar()//Only allowed when selecting a single note.
    {
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (int.TryParse(duration_bar_edit.Text, out int bar_r) && bar_r >= 0)
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration,
                new Utils.Fraction(((CameraNoteData)d).Duration.GetTrueNumerator(), ((CameraNoteData)d).Duration.Denominator) + bar_r
                ));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        Utils.Fraction end = pointing_notedatas[0].Position + pointing_notedatas[0].Duration;
        bar_edit_end.Text = end.GetWhole().ToString();
        fraction_edit_end.Text = $"{end.GetTrueNumerator()}/{end.Denominator}";

        duration_bar_edit.Text = pointing_notedatas[0].Duration.GetWhole().ToString();
        duration_fraction_edit.Text = $"{pointing_notedatas[0].Duration.GetTrueNumerator()}/{pointing_notedatas[0].Duration.Denominator}";
    }
    void change_duration_fraction()//Only allowed when selecting a single note.
    {
        string[] info = duration_fraction_edit.Text.Trim().Split('/');
        NoteMap.TryGetValue(SelectedNoteList[0], out var d);
        CameraNoteData d_cam = (CameraNoteData)d;//No type detection here.
        if (info.Length >= 2 && int.TryParse(info[0], out int num_r) && num_r >= 0 && int.TryParse(info[1], out int den_r) &&
            (num_r >= 0 && num_r < den_r && den_r > 0))
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration,
                 new Utils.Fraction(num_r, den_r) + ((CameraNoteData)d).Duration.GetWhole()
                ));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (info.Length == 1 && int.TryParse(info[0], out int r) && r == 0)
        {
            Editor.Instance.ModifyNoteData(SelectedNoteList, new ModificationRequest()
                .SetFor<CameraNoteData, Utils.Fraction>(data => data.Duration,
                 new Utils.Fraction(((CameraNoteData)d).Duration.GetWhole(), 1)
                ));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        Utils.Fraction end = pointing_notedatas[0].Position + pointing_notedatas[0].Duration;
        bar_edit_end.Text = end.GetWhole().ToString();
        fraction_edit_end.Text = $"{end.GetTrueNumerator()}/{end.Denominator}";

        duration_bar_edit.Text = pointing_notedatas[0].Duration.GetWhole().ToString();
        duration_fraction_edit.Text = $"{pointing_notedatas[0].Duration.GetTrueNumerator()}/{pointing_notedatas[0].Duration.Denominator}";
    }
}
