using Godot;
using System;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class NoteEditManager : Node
{
    static PanelContainer hitnote_panel;
    static PanelContainer bpm_note_panel;
    static PanelContainer bkg_note_panel;
    static PanelContainer default_panel;
    public override void _Ready()
    {
        hitnote_panel = this.GetParent().GetChild<PanelContainer>(0);
        bpm_note_panel = this.GetParent().GetChild<PanelContainer>(1);
        bkg_note_panel = this.GetParent().GetChild<PanelContainer>(2);
        default_panel = this.GetParent().GetChild<PanelContainer>(3);
        Editor.Instance.NoteEdits.Add(default_panel);
        Editor.Instance.NoteEdits.Add(hitnote_panel);
        Editor.Instance.NoteEdits.Add(bpm_note_panel);
        Editor.Instance.NoteEdits.Add(bkg_note_panel);
        foreach (var e in Editor.Instance.NoteEdits)
            e.Visible = false;
        default_panel.Visible = true;
        base._Ready();
    }
    public static void UpdateNoteEdits()
    {
        foreach (var e in Editor.Instance.NoteEdits)
        {
            e.Visible = false;
        }
        if (SelectedNoteList.Count <= 0)
        {
            default_panel.Visible = true;
            return;
        }
        NoteMap.TryGetValue(SelectedNoteList[0], out NoteData d0);
        if (SelectedNoteList.All(n => NoteMap.TryGetValue(n, out var d) &&
        (d.Type == d0.Type|| (HitnoteTypes.Contains(d0.Type) && HitnoteTypes.Contains(d.Type)))
        ))
        {
            if (HitnoteTypes.Contains(d0.Type))
            {
                hitnote_panel.Visible = true;
                hitnote_panel.Call("UpdateLineEdits");
            }
            else if (d0.Type == NoteType.BPM)
            {
                bpm_note_panel.Visible = true;
                bpm_note_panel.Call("UpdateLineEdits");
            }
            else if (d0.Type == NoteType.BKG)
            {
                bkg_note_panel.Visible = true;
                bkg_note_panel.Call("UpdateLineEdits");
            }
        }
        else
        {
            default_panel.Visible = true;
        }
    }
}
