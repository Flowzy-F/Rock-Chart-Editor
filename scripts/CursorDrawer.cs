using Godot;
using System;
using static Editor;
using static NoteInfo;

public partial class CursorDrawer : Control
{
    public override void _Draw()
    {
        draw_cursor();
        draw_candidate();
        draw_multiple_selection_region();
        base._Draw();
    }
    void draw_cursor()
    {
        Vector2 pos = ToLocalPosition(Editor.Instance.MousePointedLocalCoord);
        if (Editor.Instance.MousePointedLocalCoord.X < 0 || Editor.Instance.MousePointedLocalCoord.X > TrackCount - 1
            || Editor.Instance.MousePointedLocalCoord.Y - TimeOffset < 0//When TimeOffset=0,the bottom line will be drawn.Otherwise it won't be drawn.
            || pos.Y < 0)
            return;
        DrawCircle(pos, CursorSize, new Color(255, 255, 255));
    }
    void draw_candidate()
    {
        foreach (NoteHash h in CandidateNoteList)
        {
            float pos_x = 0;
            float pos_y = DrawerDisplaySize.Y -
               ((float)h.Position.Numerator / h.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom);

            if (h.NoteType == NoteType.Hit)
                pos_x = (h.Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
            else
                //Effect Notes,these notes are drawn at the last track,but there actual track is not TrackCount-1.
                pos_x = (TrackCount - 1 + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
            DrawCircle(new Vector2(pos_x, pos_y), NoteSize + 3, CandidateNoteColor, false, 2);
        }
    }
    void draw_multiple_selection_region()
    {
        if(!IsMultipleSelecting) return;
        DrawRect(Editor.Instance.MultipleSelectionRegionRect, MultipleSelectionRegionColor);
        
    }
    /// <returns>The coord on the currently displayed grid.With the bottom-left as origin.</returns>
}
