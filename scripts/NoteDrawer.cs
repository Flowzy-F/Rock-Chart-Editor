using Godot;
using System;
using System.Linq;
using static Editor;
using static NoteInfo;

public partial class NoteDrawer : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }
    public override void _Draw()
    {
        int visible_bar_count = (int)Math.Ceiling(DrawerDisplaySize.Y / (BeatHeight / Zoom)) + 1;
        for (int i = Bar; i < visible_bar_count + Bar; i++)
        {
            if (NoteMapBar.TryGetValue(i, out var list))
            {
                foreach (var h in list)
                {
                    if (NoteMap.TryGetValue(h, out var d))
                    {
                        float pos_x = (d.Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                        float pos_y = DrawerDisplaySize.Y -
                           ((float)d.Position.Numerator / d.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom);
                        float draw_scale = 1.0f;
                        Color note_color = NormalNoteColor;
                        if (HitnoteTypes.Contains(d.Type))
                        {
                            if (d.Type == NoteType.Normal)
                                note_color = NormalNoteColor;
                            else if (d.Type == NoteType.Gold)
                                note_color = GoldNoteColor;
                            string str = "";
                            string str2 = "";
                            if (((HitNoteData)d).Count > 1)
                                str += ((HitNoteData)d).Count;
                            if (((HitNoteData)d).RemoveCount > 0)
                                str += "-" + ((HitNoteData)d).RemoveCount;
                            if (((HitNoteData)d).YOffset > 0)
                                str += "↑" + ((HitNoteData)d).YOffset;
                            if (((HitNoteData)d).Scale != 1.0f)
                            {
                                str2 += "\nScale=" + ((HitNoteData)d).Scale;
                                if (((HitNoteData)d).Scale <= 0.3f)
                                    draw_scale = 0.3f;
                                else if (((HitNoteData)d).Scale >= 3f)
                                    draw_scale = 3f;
                                else
                                    draw_scale = ((HitNoteData)d).Scale;
                            }
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), str);
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 30), str2);
                        }
                        else if (d.Type == NoteType.BPM)
                        {
                            note_color = BPMNoteColor;
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), $"Change BPM to:{((BPMNoteData)d).BPMValue}");
                        }
                        else if (d.Type == NoteType.BKG)
                        {
                            note_color = BKGNoteColor;
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), "BKG");
                        }
                        DrawCircle(new Vector2(pos_x, pos_y), NoteSize * draw_scale, note_color);
                        if (SelectedNoteList.Contains(h))
                        {
                            DrawCircle(new Vector2(pos_x, pos_y), NoteSize + 3, SelectedNoteColor, false, 2);
                        }
                    }
                    else
                        throw new Exception("Notehash in NoteMapBar isn't found in NoteMap!");
                }
            }
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
