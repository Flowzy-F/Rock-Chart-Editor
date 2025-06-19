using Godot;
using System;
using System.Collections.Generic;
using static Editor;
using static NoteInfo;
public partial class NoteTriggeringManager
{
    private static List<NoteHash> triggered_notes = new List<NoteHash>();
    public static void ClearTriggeredNotes()
    {
        triggered_notes.Clear();
    }
    public static void CheckNotes()
    {
        if (NoteMapBar.TryGetValue(Bar, out var list))
        {
            foreach (NoteHash h in list)
            {
                if (!triggered_notes.Contains(h) && (Bar + TimeOffset >= (float)h.Position.Numerator / h.Position.Denominator))
                {
                    if (NoteMap.TryGetValue(h, out var d))
                    {
                        if (d.Type != NoteType.Hit) continue;
                        if (((HitNoteData)d).Color == HitColor.Normal|| ((HitNoteData)d).Color == HitColor.Performance)
                        {
                            Editor.Instance.SEPlayerNormal.Play();
                        }
                        else if (((HitNoteData)d).Color == HitColor.Gold)
                        {
                            Editor.Instance.SEPlayerGold.Play();
                        }
                        triggered_notes.Add(h);
                    }
                    else
                        throw new Exception("NoteData in NoteMapBar isn't found in NoteMap!");
                }
            }
        }
    }
    public static void InitWhenStartPlaying()
    {
        ClearTriggeredNotes();
        if (NoteMapBar.TryGetValue(Bar, out var list))
        {
            foreach (NoteHash h in list)
            {
                if (!triggered_notes.Contains(h) && Bar + TimeOffset > (float)h.Position.Numerator / h.Position.Denominator)
                {
                    if (NoteMap.TryGetValue(h, out var d))
                    {
                        triggered_notes.Add(h);
                    }
                    else
                        throw new Exception("NoteData in NoteMapBar isn't found in NoteMap!");
                }
            }
        }

    }
}
