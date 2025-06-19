using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class HitNoteEdit : PanelContainer
{
    LineEdit bar_edit;
    LineEdit fraction_edit;
    LineEdit track_edit;
    LineEdit count_edit;
    LineEdit remove_count_edit;
    LineEdit scale_edit;
    OptionButton type_option;
    LineEdit y_offset_edit;
    List<HitNoteData> pointing_notedatas = new List<HitNoteData>();
    public override void _Ready()
    {
        bar_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(1);
        fraction_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(2);
        track_edit = this.GetChild(0).GetChild(2).GetChild<LineEdit>(1);
        count_edit = this.GetChild(0).GetChild(3).GetChild<LineEdit>(1);
        remove_count_edit = this.GetChild(0).GetChild(4).GetChild<LineEdit>(1);
        scale_edit = this.GetChild(0).GetChild(5).GetChild<LineEdit>(1);
        type_option = this.GetChild(0).GetChild(6).GetChild<OptionButton>(1);
        y_offset_edit = this.GetChild(0).GetChild(7).GetChild<LineEdit>(1);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(bar_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(fraction_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(track_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(count_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(remove_count_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(scale_edit);
        Editor.Instance.FocusChangeNeedingLineEdits.Add(y_offset_edit);
        //Connection
        scale_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_scale)));
        bar_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_bar)));
        track_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_track)));
        fraction_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_fraction)));
        count_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_count)));
        remove_count_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_remove_count)));
        type_option.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(change_color)));
        y_offset_edit.Connect(LineEdit.SignalName.FocusExited, new Callable(this, nameof(change_y_offset)));
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
                if (d.Type != NoteType.Hit)
                    throw new Exception("HitnoteEdit:Poiting at a wrong sort of note!");//TODO
                pointing_notedatas.Add((HitNoteData)d);
            }
            else
                throw new Exception("NoteEdit:Pointing Note not founded in NoteMap!");
        }
        //Change Text
        bar_edit.Text = pointing_notedatas.All(n => n.Position.GetWhole() == pointing_notedatas[0].Position.GetWhole())
            ? pointing_notedatas[0].Position.GetWhole().ToString() : "...";
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
        count_edit.Text = pointing_notedatas.All(n => n.Count == pointing_notedatas[0].Count)
            ? pointing_notedatas[0].Count.ToString() : "...";
        remove_count_edit.Text = pointing_notedatas.All(n => n.RemoveCount == pointing_notedatas[0].RemoveCount)
            ? pointing_notedatas[0].RemoveCount.ToString() : "...";
        scale_edit.Text = pointing_notedatas.All(n => n.Scale == pointing_notedatas[0].Scale)
            ? pointing_notedatas[0].Scale.ToString() : "...";
        track_edit.Text = pointing_notedatas.All(n => n.Track == pointing_notedatas[0].Track)
            ? pointing_notedatas[0].Track.ToString() : "...";
        y_offset_edit.Text = pointing_notedatas.All(n => n.YOffset == pointing_notedatas[0].YOffset)
            ? pointing_notedatas[0].YOffset.ToString() : "...";

        if (pointing_notedatas.All(n => n.Color == pointing_notedatas[0].Color))
        {
            switch (pointing_notedatas[0].Color)
            {
                case HitColor.Normal:
                    type_option.Selected = 0;
                    break;
                case HitColor.Gold:
                    type_option.Selected = 1;
                    break;
                default:
                    break;
            }
            type_option.Disabled = false;
        }
        else
        {
            type_option.Disabled = true;
        }
        if (pointing_notedatas.All(n => n.Track == pointing_notedatas[0].Track))
        {
            track_edit.Text = pointing_notedatas[0].Track.ToString();
            track_edit.Editable = true;
        }
        else
        {
            track_edit.Editable = false;
        }
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
    void change_scale()
    {
        if (float.TryParse(scale_edit.Text.Trim(), out float scale_r) && scale_r >= 0)
        {
            ModificationRequest request = new ModificationRequest();
            request.SetFor<HitNoteData, float>(d => d.Scale, scale_r);
            if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        scale_edit.Text = pointing_notedatas[0].Scale.ToString();
    }
    void change_bar()//Only allowed when selecting a single note.
    {
        if (int.TryParse(bar_edit.Text.Trim(), out int bar_r) && bar_r >= 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetTrueNumerator() + bar_r * SelectedNoteList[0].Position.Denominator
                , SelectedNoteList[0].Position.Denominator), NoteType.Hit, SelectedNoteList[0].Track)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        bar_edit.Text = pointing_notedatas[0].Position.GetWhole().ToString();
    }
    void change_count()//Only allowed when selecting a single note.
    {
        if (int.TryParse(count_edit.Text.Trim(), out int count_r) && count_r > 0)
        {
            ModificationRequest request = new ModificationRequest();
            request.SetFor<HitNoteData, int>(d => d.Count, count_r);
            if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        count_edit.Text = pointing_notedatas[0].Count.ToString();
    }
    void change_remove_count()//Only allowed when selecting a single note.
    {
        if (int.TryParse(remove_count_edit.Text.Trim(), out int remove_count_r) && remove_count_r >= 0)
        {
            ModificationRequest request = new ModificationRequest();
            request.SetFor<HitNoteData, int>(d => d.RemoveCount, remove_count_r);
            if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        remove_count_edit.Text = pointing_notedatas[0].RemoveCount.ToString();
    }
    void change_color(int selected)//Only allowed when selecting a single note.
    {
        HitColor target_color = HitColor.Normal;
        switch (selected)
        {
            case 0:
                target_color = HitColor.Normal;
                break;
            case 1:
                target_color = HitColor.Gold;
                break;
            case 2:
                target_color = HitColor.Performance;
                break;
            default:
                break;
        }
        ModificationRequest request=new ModificationRequest();
        request.SetFor<HitNoteData,HitColor>(d => d.Color, target_color);
        if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
            Editor.Instance.NoteDrawer.QueueRedraw();
        switch (pointing_notedatas[0].Color)
        {
            case HitColor.Normal:
                type_option.Selected = 0;
                break;
            case HitColor.Gold:
                type_option.Selected = 1;
                break;
            default:
                break;
        }
    }
    void change_track()
    {
        //UNDONE:TrackCount-SPNoteOffset.Count=Amount of SP Note type ?
        if (float.TryParse(track_edit.Text.Trim(), out float track_r) && track_r >= TrackLimitLeft && track_r <= TrackLimitRight)
        {
            if (Editor.Instance.MoveNoteTo(SelectedNoteList[0],new NoteHash(  SelectedNoteList[0].Position,NoteType.Hit, track_r)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        track_edit.Text = pointing_notedatas[0].Track.ToString();
    }
    void change_fraction()//Only allowed when selecting a single note.
    {
        string[] info = fraction_edit.Text.Trim().Split('/');
        if (info.Length >= 2 && int.TryParse(info[0], out int num_r) && num_r >= 0 && int.TryParse(info[1], out int den_r) &&
            (num_r >= 0 && num_r < den_r && den_r > 0))
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole() * den_r + num_r, den_r), NoteType.Hit, SelectedNoteList[0].Track)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else if (int.TryParse(fraction_edit.Text.Trim(), out int r) && r == 0)
        {
            if (
            Editor.Instance.MoveNoteTo(SelectedNoteList[0], new NoteHash(
                new Utils.Fraction(SelectedNoteList[0].Position.GetWhole(), SelectedNoteList[0].Position.Numerator),NoteType.Hit, SelectedNoteList[0].Track)))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        fraction_edit.Text = $"{pointing_notedatas[0].Position.GetTrueNumerator()}/{pointing_notedatas[0].Position.Denominator}";
    }
    void change_y_offset()
    {
        if (int.TryParse(y_offset_edit.Text.Trim(), out int yoffset_r) && yoffset_r >= 0)
        {
            ModificationRequest request = new ModificationRequest();
            request.SetFor<HitNoteData,int>(d => d.YOffset, yoffset_r);
            if (Editor.Instance.ModifyNoteData(SelectedNoteList, request))
                Editor.Instance.NoteDrawer.QueueRedraw();
        }
        else
        {
            //TODO
        }
        y_offset_edit.Text = pointing_notedatas[0].YOffset.ToString();
    }
}
