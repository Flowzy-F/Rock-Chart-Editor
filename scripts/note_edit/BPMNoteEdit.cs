using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class BPMNoteEdit : PanelContainer
{
    LineEdit bar_edit;
    LineEdit fraction_edit;
    LineEdit bpm_edit;
    List<BPMNoteData> pointing_notedatas = new List<BPMNoteData>();
    public override void _Ready()
    {
        bar_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(1);
        fraction_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(2);
        bpm_edit = this.GetChild(0).GetChild(2).GetChild<LineEdit>(1);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bar_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(fraction_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bpm_edit);
        //Connection
        bar_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bar)));
        fraction_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_fraction)));
        bpm_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bpm)));
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
                if (d.Type != NoteType.BPM)
                    throw new Exception("BPMNoteEdit:Poiting at a wrong sort of note!");
                pointing_notedatas.Add((BPMNoteData)d);
            }
            else
                throw new Exception("NoteEdit:Pointing Note not founded in NoteMap!");
        }
        //Change Text
        bar_edit.Text = pointing_notedatas.All(n => n.Position.GetWhole() == pointing_notedatas[0].Position.GetWhole())
            ? pointing_notedatas[0].Position.GetWhole().ToString() : "...";
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
        bpm_edit.Text = pointing_notedatas.All(n => n.BPMValue == pointing_notedatas[0].BPMValue)
            ? pointing_notedatas[0].BPMValue.ToString() : "...";
        if (pointing_notedatas.Count == 1)
        {
            bar_edit.Editable = true;//TODO:Bar change is only allowed when selecting a single note.
            fraction_edit.Editable = true;
            bpm_edit.Editable = true;
        }
        else
        {
            bar_edit.Editable = false;
            fraction_edit.Editable = false;
            bpm_edit.Editable = false;
        }
    }
    void change_bpm()
    {
        if (float.TryParse(bpm_edit.Text.Trim(), out float bpm_r) && bpm_r > 0)
        {
            ModificationRequest request= new ModificationRequest().SetFor<BPMNoteData,float>(d=>d.BPMValue,bpm_r);
            if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
                SyncTimeSystem.BuildBPMTimeLine();
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        bpm_edit.Text = pointing_notedatas[0].BPMValue.ToString();
        SyncTimeSystem.BuildBPMTimeLine();
    }
    void change_bar()//Only allowed when selecting a single note.
    {
        if (int.TryParse(bar_edit.Text.Trim(), out int bar_r) && bar_r >= 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetTrueNumerator() + bar_r * SelectedNoteList[0].Position.Denominator
                , SelectedNoteList[0].Position.Denominator), NoteType.BPM)))
            {
                SyncTimeSystem.BuildBPMTimeLine();
                Editor.Instance.NoteDrawer.QueueRedraw();
            }
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
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole() * den_r + num_r, den_r), NoteType.BPM));
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (int.TryParse(fraction_edit.Text.Trim(), out int r) && r == 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole(), SelectedNoteList[0].Position.Numerator), NoteType.BPM)))
            {
                SyncTimeSystem.BuildBPMTimeLine();
                Editor.Instance.NoteDrawer.QueueRedraw();
            }
        }
        else
        {
            //TODO
        }
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
    }
}
