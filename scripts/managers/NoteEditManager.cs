using Godot;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class NoteEditManager : Node
{
    static PanelContainer hitnote_panel;
    static PanelContainer camera_note_panel;
    static PanelContainer bpm_note_panel;
    static PanelContainer bkg_note_panel;
    static PanelContainer default_panel;
    public override void _Ready()
    {
        hitnote_panel = this.GetParent().GetChild<PanelContainer>(0);
        camera_note_panel = this.GetParent().GetChild<PanelContainer>(1);
        bpm_note_panel = this.GetParent().GetChild<PanelContainer>(2);
        bkg_note_panel = this.GetParent().GetChild<PanelContainer>(3);
        default_panel = this.GetParent().GetChild<PanelContainer>(4);
        Editor.Instance.NoteEdits.Add(NoteType.Invalid,default_panel);
        Editor.Instance.NoteEdits.Add(NoteType.Camera,camera_note_panel);
        Editor.Instance.NoteEdits.Add(NoteType.Hit,hitnote_panel);
        Editor.Instance.NoteEdits.Add(NoteType.BPM,bpm_note_panel);
        Editor.Instance.NoteEdits.Add(NoteType.BKG, bkg_note_panel);
        foreach (var e in Editor.Instance.NoteEdits.Values)
            e.Visible = false;
        default_panel.Visible = true;
        base._Ready();
    }
    public static void UpdateNoteEdits()
    {
        foreach (var e in Editor.Instance.NoteEdits.Values)
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
        (d.Type == d0.Type)))
        {
            Editor.Instance.NoteEdits[d0.Type].Visible = true;
            Editor.Instance.NoteEdits[d0.Type].Call("UpdateLineEdits");
        }
        else
        {
            default_panel.Visible = true;
        }
    }
}
