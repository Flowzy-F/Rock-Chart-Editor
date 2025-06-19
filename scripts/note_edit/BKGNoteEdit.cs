using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class BKGNoteEdit : PanelContainer
{
    LineEdit bar_edit;
    LineEdit fraction_edit;
    List<BKGNoteData> pointing_notedatas = new List<BKGNoteData>();
    public override void _Ready()
    {
        bar_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(1);
        fraction_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(2);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bar_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(fraction_edit);
        //Connection
        bar_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bar)));
        fraction_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_fraction)));
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
                if (d.Type!=NoteType.BKG)
                    throw new Exception("BKGNoteEdit:Poiting at a wrong sort of note!");//TODO
                pointing_notedatas.Add((BKGNoteData)d);
            }
            else
                throw new Exception("NoteEdit:Pointing Note not founded in NoteMap!");
        }
        //Change Text
        bar_edit.Text = pointing_notedatas.All(n => n.Position.GetWhole() == pointing_notedatas[0].Position.GetWhole())
            ? pointing_notedatas[0].Position.GetWhole().ToString() : "...";
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
        if (pointing_notedatas.Count == 1)
        {
            bar_edit.Editable = true;//TODO:Bar change is only allowed when selecting a single note.
            fraction_edit.Editable = true;
        }
        else
        {
            bar_edit.Editable = false;
            fraction_edit.Editable = false;
        }
    }
    void change_bar()//Only allowed when selecting a single note.
    {
        if (int.TryParse(bar_edit.Text.Trim(), out int bar_r) && bar_r >= 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetTrueNumerator() + bar_r * SelectedNoteList[0].Position.Denominator
                , SelectedNoteList[0].Position.Denominator),NoteType.BKG)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        bar_edit.Text = pointing_notedatas[0].Position.GetWhole().ToString();
    }
    void change_fraction()//Only allowed when selecting a single note.
    {
        string[] info = fraction_edit.Text.Trim().Split('/');
        if (info.Length >= 2 && int.TryParse(info[0], out int num_r) && num_r >= 0 && int.TryParse(info[1], out int den_r) &&
            (num_r >= 0 && num_r < den_r && den_r > 0))
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole() * den_r + num_r, den_r),NoteType.BKG)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (int.TryParse(fraction_edit.Text.Trim(), out int r) && r == 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole(), SelectedNoteList[0].Position.Numerator),NoteType.BKG)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
    }
}
