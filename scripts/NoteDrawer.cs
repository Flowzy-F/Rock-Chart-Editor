using Godot;
using System;
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
                    //Init pos_x is for effect notes at the last track.
                    float pos_x = (TrackCount - 1 + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                    float pos_y = DrawerDisplaySize.Y -
                       ((float)h.Position.Numerator / h.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom);
                    float draw_scale = 1.0f;
                    Color note_color = NormalNoteColor;
                    if (h.NoteType == NoteType.Hit)
                    {

                        if (!NoteMap.TryGetValue(h, out var d))
                            throw new Exception("Notehash in NoteMapBar isn't found in NoteMap!");
                        string str = "";
                        string str2 = "";

                        pos_x = (((HitNoteData)d).Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                        if (((HitNoteData)d).Color == HitColor.Normal)
                            note_color = NormalNoteColor;
                        else if (((HitNoteData)d).Color == HitColor.Gold)
                            note_color = GoldNoteColor;
                        else if (((HitNoteData)d).Color == HitColor.Performance)
                        {
                            str += "[Auto]";
                            note_color = PerformanceNoteColor;
                        }
                        if (((HitNoteData)d).Count > 1)
                            str += ((HitNoteData)d).Count;
                        if (((HitNoteData)d).RemoveCount > 0)
                            str += "-" + ((HitNoteData)d).RemoveCount;
                        if (((HitNoteData)d).YOffset > 0)
                            str += "↑" + ((HitNoteData)d).YOffset;
                        if (((HitNoteData)d).Scale != 1.0f)
                        {
                            str2 += "\n缩放=" + ((HitNoteData)d).Scale;
                            if (((HitNoteData)d).Scale <= 0.3f)
                                draw_scale = 0.3f;
                            else if (((HitNoteData)d).Scale >= 3f)
                                draw_scale = 3f;
                            else
                                draw_scale = ((HitNoteData)d).Scale;
                        }
                        DrawCircle(new Vector2(pos_x, pos_y), NoteSize, note_color);
                        DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), str);
                        DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 30), str2);
                    }
                    else if(h.NoteType == DisplayedEffectNoteType)
                    {
                         if (h.NoteType == NoteType.BPM)
                        {
                            if (!NoteMap.TryGetValue(h, out var d))
                                throw new Exception("Notehash in NoteMapBar isn't found in NoteMap!");
                            note_color = BPMNoteColor;
                            DrawCircle(new Vector2(pos_x, pos_y), NoteSize, note_color);
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), $"BPM变化为:{((BPMNoteData)d).BPMValue}");
                        }
                        else if (h.NoteType == NoteType.BKG )
                        {
                            if (!NoteMap.TryGetValue(h, out var d))
                                throw new Exception("Notehash in NoteMapBar isn't found in NoteMap!");
                            note_color = BKGNoteColor;
                            DrawCircle(new Vector2(pos_x, pos_y), NoteSize, note_color);
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), "BKG");
                        }
                        else if (h.NoteType == NoteType.Camera )
                        {
                            if (!NoteMap.TryGetValue(h, out var d))
                                throw new Exception("Notehash in NoteMapBar isn't found in NoteMap!");
                            Utils.Fraction end = ((CameraNoteData)d).Position + ((CameraNoteData)d).Duration;
                            float end_y = DrawerDisplaySize.Y -
                               ((float)end.Numerator / end.Denominator -
                               (Bar + TimeOffset)) * (BeatHeight / Zoom);
                            note_color = CameraNoteColor;
                            DrawCircle(new Vector2(pos_x, pos_y), NoteSize, note_color);
                            DrawRect(new Rect2(new Vector2(pos_x - NoteSize/4, end_y), new Vector2(NoteSize/2, pos_y - end_y)), note_color);
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 15), "相机运动");
                            DrawString(Editor.Instance.DefaultFont, new Vector2(pos_x + 10, pos_y + 30), ((CameraNoteData)d).Easing.ToString());
                        }
                    }

                    if (SelectedNoteList.Contains(h))
                    {
                        DrawCircle(new Vector2(pos_x, pos_y), NoteSize + 3, SelectedNoteColor, false, 2);
                    }
                }
            }
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
