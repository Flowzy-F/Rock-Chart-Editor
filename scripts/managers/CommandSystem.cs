using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static Utils;
using static NoteInfo;

public interface ICommand
{
    bool Execute();
    void Undo();
    void Redo() => Execute();
    string DisplayName { get; }
}
public class AddHitNoteCommand : ICommand
{
    private Dictionary<HitNoteData, bool> target_datas = new Dictionary<HitNoteData, bool>();//Target Data,If successfully added.
    private bool add_count;
    public string DisplayName => $"Added {target_datas.Count} Hit Note(s).";
    public AddHitNoteCommand(List<HitNoteData> target_datas, bool add_count)
    {
        if (target_datas.Any(target_data => !HitnoteTypes.Contains(target_data.Type)))
            throw new Exception("A HitNoteData's type is not Hitnote!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add((HitNoteData)d.Clone(), true);
        }
        this.add_count = add_count;
    }

    public bool Execute()
    {
        bool executed = false;//When there is a note on the target position and (!add_count),this command is actually not executed.
                              //This will be set to true when any note is added successfully.
        if (target_datas.Count <= 0) return false;
        if (target_datas.Keys.Any(target_data => target_data.Track < -0.5f || target_data.Track > TrackCount - SPNoteOffset.Count - 0.5f)) return false;
        foreach (var target_data in target_datas.Keys)
        {
            if (NoteMap.TryGetValue(target_data.GetNotehash(), out NoteData d))
            {
                if (add_count && d.Type == target_data.Type)
                {
                    ((HitNoteData)d).Count += (target_data).Count;
                    executed = true;
                    continue;
                }
                else
                    target_datas[target_data] = false;
            }
            else
            {
                NoteHash h = target_data.GetNotehash();
                //Map
                NoteMap.Add(h, target_data.Clone());
                //Map Bar
                if (NoteMapBar.TryGetValue(target_data.Position.GetWhole(), out var list))
                    list.Add(h);
                else
                {
                    List<NoteHash> new_list = new List<NoteHash>();
                    new_list.Add(h);
                    NoteMapBar.Add(target_data.Position.GetWhole(), new_list);
                }
                executed = true;
            }
        }
        if (!executed) return false;
        return true;
    }
    public void Undo()
    {
        foreach (var target_data in target_datas.Keys)
        {
            if (target_datas[target_data] == false) continue;
            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                if (add_count)
                {
                    ((HitNoteData)d).Count -= target_data.Count;
                    if (((HitNoteData)d).Count > 0) continue;
                }
                //Map
                NoteMap.Remove(delete_hash);

                //MapBar
                NoteMapBar.TryGetValue(delete_hash.Position.GetWhole(), out var list);
                list.Remove(delete_hash);
                if (list.Count <= 0)
                {
                    NoteMapBar.Remove(delete_hash.Position.GetWhole());
                }
                //SelectAndCandidate
                SelectedNoteList.Remove(delete_hash);
                CandidateNoteList.Remove(delete_hash);
            }
            else
                throw new Exception("Added note not found when undoing!Do not modify NoteMap outside CommandSystem!");
        }
    }
}
public class AddBPMNoteCommand : ICommand
{
    private Dictionary<BPMNoteData, bool> target_datas = new Dictionary<BPMNoteData, bool>();//Target Data,If Successfully Added
    private bool rebuild_bpm_timeline;
    private float origin_bpm = -1;//If this command modified a BPM Note,get it back when undoing.
    public string DisplayName => $"Added {target_datas.Count} BPM Note(s).";
    public AddBPMNoteCommand(List<BPMNoteData> target_datas, bool rebuild_bpm_timeline)
    {
        if (target_datas.Any(target_data => target_data.Type != NoteType.BPM))
            throw new Exception("A BPMNoteData's type is not NoteType.BPM!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add((BPMNoteData)d.Clone(), true);
        }
        this.rebuild_bpm_timeline = rebuild_bpm_timeline;
    }

    public bool Execute()
    {
        if (target_datas.Count <= 0) return false;
        if (target_datas.Keys.Any(target_data => target_data.Track != TrackCount - SPNoteOffset["BPM"])) return false;
        foreach (var target_data in target_datas.Keys)
        {
            if (NoteMap.TryGetValue(new NoteHash(target_data.Position, TrackCount - SPNoteOffset["BPM"]), out NoteData data))
            {
                origin_bpm = ((BPMNoteData)data).BPMValue;
                ((BPMNoteData)data).BPMValue = target_data.BPMValue;
            }
            else
            {
                NoteHash h = target_data.GetNotehash();
                //Map
                NoteMap.Add(h, target_data.Clone());
                //Map Bar
                if (NoteMapBar.TryGetValue(target_data.Position.GetWhole(), out var list))
                    list.Add(h);
                else
                {
                    List<NoteHash> new_list = new List<NoteHash>();
                    new_list.Add(h);
                    NoteMapBar.Add(target_data.Position.GetWhole(), new_list);
                }
                //BPM Note List
                BPMNoteList.Add(h);
            }
        }
        if (rebuild_bpm_timeline)
            SyncTimeSystem.BuildBPMTimeLine();
        return true;
    }
    public void Undo()
    {
        foreach (var target_data in target_datas.Keys)
        {
            if (target_datas[target_data] == false) continue;
            if (!NoteMap.ContainsKey(target_data.GetNotehash()))
                throw new Exception("Added note not found when undoing!Do not modify NoteMap outside CommandSystem!");
            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                if (origin_bpm > 0)
                {
                    ((BPMNoteData)d).BPMValue = origin_bpm;
                    continue;
                }
                //Map
                NoteMap.Remove(delete_hash);

                //MapBar
                NoteMapBar.TryGetValue(delete_hash.Position.GetWhole(), out var list);
                list.Remove(delete_hash);
                if (list.Count <= 0)
                {
                    NoteMapBar.Remove(delete_hash.Position.GetWhole());
                }
                //SelectAndCandidate
                SelectedNoteList.Remove(delete_hash);
                CandidateNoteList.Remove(delete_hash);
                //BPM
                BPMNoteList.Remove(delete_hash);
            }
        }
        if (rebuild_bpm_timeline) SyncTimeSystem.BuildBPMTimeLine();
    }
}
public class AddBKGNoteCommand : ICommand
{
    private Dictionary<NoteData, bool> target_datas = new Dictionary<NoteData, bool>();//Target Data,If Successfully Added
    public string DisplayName => $"Added {target_datas.Count} BKG Note(s).";
    public AddBKGNoteCommand(List<NoteData> target_datas)
    {
        if (target_datas.Any(target_data => target_data.Type != NoteType.BKG))
            throw new Exception("A BKGNoteData's type is not NoteType.BKG!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add(d.Clone(), true);
        }
    }

    public bool Execute()
    {
        bool executed = false;//When there is a note on the target position ,this command is actually not executed.
                              //This will be set to true when any note is added successfully.
        if (target_datas.Count <= 0) return false;
        if (target_datas.Keys.Any(target_data => target_data.Track != TrackCount - SPNoteOffset["BKG"])) return false;
        foreach (var target_data in target_datas.Keys)
        {
            if (NoteMap.ContainsKey(target_data.GetNotehash()))
            {
                target_datas[target_data] = false;
                continue;
            }
            NoteHash h = target_data.GetNotehash();
            //Map
            NoteMap.Add(h, target_data.Clone());
            //Map Bar
            if (NoteMapBar.TryGetValue(target_data.Position.GetWhole(), out var list))
                list.Add(h);
            else
            {
                List<NoteHash> new_list = new List<NoteHash>();
                new_list.Add(h);
                NoteMapBar.Add(target_data.Position.GetWhole(), new_list);
            }
            executed = true;
        }
        if (!executed) return false;
        return true;
    }
    public void Undo()
    {
        foreach (var target_data in target_datas.Keys)
        {
            if (target_datas[target_data] == false) continue;
            if (!NoteMap.ContainsKey(target_data.GetNotehash()))
                throw new Exception("Added note not found when undoing!Do not modify NoteMap outside CommandSystem!");
            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                //Map
                NoteMap.Remove(delete_hash);

                //MapBar
                NoteMapBar.TryGetValue(delete_hash.Position.GetWhole(), out var list);
                list.Remove(delete_hash);
                if (list.Count <= 0)
                {
                    NoteMapBar.Remove(delete_hash.Position.GetWhole());
                }
                //SelectAndCandidate
                SelectedNoteList.Remove(delete_hash);
                CandidateNoteList.Remove(delete_hash);
            }
        }
    }
}
public class AddNoteCommand : ICommand
{
    private List<ICommand> commands = new List<ICommand>();

    private List<HitNoteData> hit_notes = new List<HitNoteData>();
    private List<BPMNoteData> bpm_notes = new List<BPMNoteData>();
    private List<NoteData> bkg_notes = new List<NoteData>();
    public string DisplayName => $"Added {hit_notes.Count + bpm_notes.Count + bkg_notes.Count} Note(s).";
    public AddNoteCommand(List<NoteData> target_datas, bool add_count = true, bool rebuild_bpm_timeline = true)
    {
        //Init commands
        foreach (var d in target_datas.Where(n => HitnoteTypes.Contains(n.Type)))
        {
            hit_notes.Add((HitNoteData)d);
        }
        foreach (var d in target_datas.Where(n => n.Type == NoteType.BPM).ToList())
        {
            bpm_notes.Add((BPMNoteData)d);
        }
        foreach (var d in target_datas.Where(n => n.Type == NoteType.BKG).ToList())
        {
            bkg_notes.Add(d);
        }
        if (hit_notes.Count > 0)
        {
            AddHitNoteCommand cmd = new AddHitNoteCommand(hit_notes, add_count);
            commands.Add(cmd);
        }
        if (bpm_notes.Count > 0)
        {
            AddBPMNoteCommand cmd = new AddBPMNoteCommand(bpm_notes, rebuild_bpm_timeline);
            commands.Add(cmd);
        }
        if (bkg_notes.Count > 0)
        {
            AddBKGNoteCommand cmd = new AddBKGNoteCommand(bkg_notes);
            commands.Add(cmd);
        }
    }
    public bool Execute()
    {
        if (commands.Count <= 0) return false;
        bool executed = false;
        foreach (var cmd in commands)
        {
            if (cmd.Execute()) executed = true;
        }
        if (!executed) return false;
        return true;
    }
    public void Undo()
    {
        if (commands.Count <= 0) return;
        foreach (var cmd in commands) { cmd.Undo(); }
    }
}
public class DeleteNotesCommand : ICommand
{
    private Dictionary<NoteHash, NoteData> targets = new Dictionary<NoteHash, NoteData>();
    private bool rebuild_bpm_timeline = true;
    private int count;
    public string DisplayName => $"Deleted {targets.Count} Notes.";
    public DeleteNotesCommand(List<NoteHash> hashes, bool rebuild_bpm_timeline, int count = -1)
    {
        foreach (var h in hashes)
        {
            if (NoteMap.TryGetValue(h, out var d))
            {
                targets.Add(h, d.Clone());
            }
        }

        this.rebuild_bpm_timeline = rebuild_bpm_timeline;
        this.count = count;
    }
    public bool Execute()
    {
        foreach (var h in targets.Keys)
        {
            if (NoteMap.TryGetValue(h, out var d))
            {
                if (count > 0 && HitnoteTypes.Contains(d.Type))
                {
                    ((HitNoteData)d).Count -= count;
                    if (((HitNoteData)d).Count > 0) continue;
                }
                //Map
                NoteMap.Remove(h);

                //MapBar
                NoteMapBar.TryGetValue(h.Position.GetWhole(), out var list);
                list.Remove(h);
                if (list.Count <= 0)
                {
                    NoteMapBar.Remove(h.Position.GetWhole());
                }
                //SelectAndCandidate
                if (SelectedNoteList.Remove(h))
                    NoteEditManager.UpdateNoteEdits();
                CandidateNoteList.Remove(h);

                //BPM
                if (d.Type == NoteType.BPM)
                {
                    BPMNoteList.Remove(h);
                }
                CandidateNoteList.Remove(h);
                SelectedNoteList.Remove(h);
            }

            else
                throw new Exception("Deleting note isn't found in NoteMap!");

        }
        if (rebuild_bpm_timeline && targets.Values.Any(n => n.Type == NoteType.BPM))
            SyncTimeSystem.BuildBPMTimeLine();
        return true;
    }
    public void Undo()
    {
        foreach (var p in targets)
        {

            if (NoteMap.TryGetValue(p.Key, out var d))
            {
                if (count > 0 && HitnoteTypes.Contains(d.Type))
                {
                    ((HitNoteData)d).Count += count;
                    if (((HitNoteData)d).Count > 0) continue;
                }
                else
                    throw new Exception("Deleted note's position has been occupied when undoing!");
            }
            //Map
            NoteMap.Add(p.Key, p.Value);

            //MapBar
            if (NoteMapBar.TryGetValue(p.Key.Position.GetWhole(), out var list))
            {
                list.Add(p.Key);
            }
            else
            {
                List<NoteHash> new_list = new List<NoteHash>();
                new_list.Add(p.Key);
                NoteMapBar.Add(p.Key.Position.GetWhole(), new_list);
            }
            if (p.Value.Type == NoteType.BPM)
            {
                BPMNoteList.Add(p.Key);
            }
        }
        if (rebuild_bpm_timeline && targets.Values.Any(n => n.Type == NoteType.BPM))
            SyncTimeSystem.BuildBPMTimeLine();
    }
}
public class MoveNoteCommand : ICommand
{
    private Dictionary<NoteHash, bool> target_notes = new Dictionary<NoteHash, bool>();//Target Hash,If Successfullt Moved
    private Dictionary<NoteHash, NoteHash> moved_hashes = new Dictionary<NoteHash, NoteHash>();//Hash after moving,Origin hash.
    private Fraction pos_addition;
    private int track_addition;
    public string DisplayName => $"Moved {target_notes.Count} notes.";
    public MoveNoteCommand(List<NoteHash> target_notes,
         Fraction pos_addition, int track_addition)
    {
        foreach (var h in target_notes)
        {
            this.target_notes.Add(h, true);
        }
        this.pos_addition = pos_addition;
        this.track_addition = track_addition;
    }
    public bool Execute()
    {
        IOrderedEnumerable<NoteHash> ordered_hashes;
        bool moved = false;//If no notes are moved,this command is actually not executed.
                           //This will be set to true when any note is successfully moved.
        if (pos_addition >= Fraction.Zero())
            ordered_hashes = target_notes.Keys.OrderByDescending(n => (float)n.Position.Numerator / n.Position.Denominator);
        else
            ordered_hashes = target_notes.Keys.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator);
        if (track_addition >= 0)
            ordered_hashes = ordered_hashes.ThenByDescending(n => n.Track);
        else
            ordered_hashes = ordered_hashes.ThenBy(n => n.Track);

        foreach (var h in ordered_hashes)
        {
            if (!NoteMap.TryGetValue(h, out var d))
            {
                throw new Exception("Moving note is not found in NoteMap!");
            }
            bool is_hit_note = HitnoteTypes.Contains(d.Type);
            NoteHash new_hash = new NoteHash(h.Position + pos_addition, h.Track + (is_hit_note ? track_addition : 0));
            if (is_hit_note)
            {
                if (new_hash.Track < -0.5f) new_hash = new NoteHash(new_hash.Position, -0.5f);
                if (new_hash.Track > TrackCount - SPNoteOffset.Count - 0.5f) new_hash = new NoteHash(new_hash.Position, TrackCount - SPNoteOffset.Count - 0.5f);
            }
            //UNDONE:SPNoteOffset.Count=Amount of SP Note types?
            if (new_hash.Position.Numerator < 0 || new_hash.Track < -0.5f ||
                (new_hash.Track > TrackCount - SPNoteOffset.Count - 0.5f && BottomPosition.Equals(Fraction.Zero()))//Only hit notes move horizontal
                || (d.Type == NoteType.BPM && new_hash.Position == Fraction.Zero())
                || NoteMap.ContainsKey(new_hash))
            //UNDONE:TrackCount - SPNoteOffset.Count=sp notetype count?
            {
                target_notes[h] = false;
                continue;
            }
            NoteMap.Remove(h);
            d.Position = new_hash.Position;
            d.Track = new_hash.Track;
            NoteMap.Add(new_hash, d);
            if (NoteMapBar.TryGetValue(h.Position.GetWhole(), out var list))
            {
                if (new_hash.Position.GetWhole() == h.Position.GetWhole())
                    list[list.FindIndex(n => n.Equals(h))] = new_hash;
                else
                {
                    list.Remove(h);
                    if (NoteMapBar.TryGetValue(new_hash.Position.GetWhole(), out var list1))
                        list1.Add(new_hash);
                    else
                    {
                        List<NoteHash> new_list = new List<NoteHash>();
                        new_list.Add(new_hash);
                        NoteMapBar.Add(new_hash.Position.GetWhole(), new_list);
                    }
                }
            }
            if (d.Type == NoteType.BPM)
            {
                BPMNoteList.Remove(h);
                BPMNoteList.Add(new_hash);
            }

            if (CandidateNoteList.Contains(h))
            {
                CandidateNoteList[CandidateNoteList.FindIndex(n => n.Equals(h))] = new_hash;
            }
            if (SelectedNoteList.Contains(h))
            {
                SelectedNoteList[SelectedNoteList.FindIndex(n => n.Equals(h))] = new_hash;
            }
            target_notes.Remove(h);
            target_notes.Add(new_hash, true);
            moved_hashes.Add(new_hash, h);
            moved = true;
        }
        if (!moved) return false;
        if (target_notes.Keys.Any(n => NoteMap.TryGetValue(n, out var d) && d.Type == NoteType.BPM && target_notes[n] == true))
            SyncTimeSystem.BuildBPMTimeLine();
        NoteEditManager.UpdateNoteEdits();
        return true;
    }
    public void Undo()
    {
        IOrderedEnumerable<NoteHash> ordered_hashes;
        if (pos_addition >= Fraction.Zero())
            ordered_hashes = target_notes.Keys.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator);
        else
            ordered_hashes = target_notes.Keys.OrderByDescending(n => (float)n.Position.Numerator / n.Position.Denominator);
        if (track_addition >= 0)
            ordered_hashes = ordered_hashes.ThenBy(n => n.Track);
        else
            ordered_hashes = ordered_hashes.ThenByDescending(n => n.Track);
        foreach (var h in ordered_hashes)
        {
            if (target_notes[h] == false) continue;
            if (!NoteMap.TryGetValue(h, out var d))
            {
                throw new Exception("Moved note is not found in NoteMap!");
            }
            bool is_hit_note = HitnoteTypes.Contains(d.Type);
            moved_hashes.TryGetValue(h, out var origin_hash);
            d.Position = origin_hash.Position;
            d.Track = origin_hash.Track;
            NoteMap.Add(origin_hash, d);
            if (NoteMapBar.TryGetValue(h.Position.GetWhole(), out var list))
            {
                if (origin_hash.Position.GetWhole() == h.Position.GetWhole())
                    list[list.FindIndex(n => n.Equals(h))] = origin_hash;
                else
                {
                    list.Remove(h);
                    if (NoteMapBar.TryGetValue(origin_hash.Position.GetWhole(), out var list1))
                        list1.Add(origin_hash);
                    else
                    {
                        List<NoteHash> new_list = new List<NoteHash>();
                        new_list.Add(origin_hash);
                        NoteMapBar.Add(origin_hash.Position.GetWhole(), new_list);
                    }
                }
            }
            if (d.Type == NoteType.BPM)
            {
                BPMNoteList.Remove(h);
                BPMNoteList.Add(origin_hash);
            }
            if (CandidateNoteList.Contains(h))
            {
                CandidateNoteList[CandidateNoteList.FindIndex(n => n.Equals(h))] = origin_hash;
            }
            if (SelectedNoteList.Contains(h))
            {
                SelectedNoteList[SelectedNoteList.FindIndex(n => n.Equals(h))] = origin_hash;
            }
            target_notes.Remove(h);
            target_notes.Add(origin_hash, true);
        }
    }
}
public class MoveNoteToCommand : ICommand
{
    private NoteHash target_hash;//Target Hash,If Successfullt Moved
    private Fraction target_pos;
    private float target_track;
    public string DisplayName => $"Moved a note to:Track {target_track},{target_pos}.";
    public MoveNoteToCommand(NoteHash target_hash,
         Fraction target_pos, float target_track)
    {
        this.target_hash = target_hash;
        this.target_pos = target_pos;
        this.target_track = target_track;
    }
    public bool Execute()
    {
        if (!NoteMap.TryGetValue(target_hash, out var d))
        {
            throw new Exception("Moving note is not found in NoteMap!");
        }
        if (NoteMap.ContainsKey(new NoteHash(target_pos, target_track))) return false;
        if (target_pos < Fraction.Zero() || (target_pos == Fraction.Zero() && d.Type == NoteType.BPM)) return false;
        if (target_track < -0.5f) return false;
        if (!HitnoteTypes.Contains(d.Type) && target_track != d.Track) return false;
        if (HitnoteTypes.Contains(d.Type) && target_track > TrackCount - SPNoteOffset.Count - 0.5f) return false;//UNDONE: SPNoteOffset.Count=Amount of SP Notes' types?
        NoteHash new_hash = new NoteHash(target_pos, target_track);
        NoteMap.Remove(target_hash);
        d.Position = new_hash.Position;
        d.Track = new_hash.Track;
        NoteMap.Add(new_hash, d);
        if (NoteMapBar.TryGetValue(target_hash.Position.GetWhole(), out var list))
        {
            if (new_hash.Position.GetWhole() == target_hash.Position.GetWhole())
                list[list.FindIndex(n => n.Equals(target_hash))] = new_hash;
            else
            {
                list.Remove(target_hash);
                if (NoteMapBar.TryGetValue(new_hash.Position.GetWhole(), out var list1))
                    list1.Add(new_hash);
                else
                {
                    List<NoteHash> new_list = new List<NoteHash>();
                    new_list.Add(new_hash);
                    NoteMapBar.Add(new_hash.Position.GetWhole(), new_list);
                }
            }
        }
        if (d.Type == NoteType.BPM)
        {
            BPMNoteList.Remove(target_hash);
            BPMNoteList.Add(new_hash);
        }
        if (CandidateNoteList.Contains(target_hash))
        {
            CandidateNoteList[CandidateNoteList.FindIndex(n => n.Equals(target_hash))] = new_hash;
        }
        if (SelectedNoteList.Contains(target_hash))
        {
            SelectedNoteList[SelectedNoteList.FindIndex(n => n.Equals(target_hash))] = new_hash;
        }
        if (NoteMap.TryGetValue(target_hash, out var data) && data.Type == NoteType.BPM)
            SyncTimeSystem.BuildBPMTimeLine();
        NoteEditManager.UpdateNoteEdits();
        return true;
    }
    public void Undo()
    {
        NoteHash moved_hash = new NoteHash(target_pos, target_track);
        if (!NoteMap.TryGetValue(moved_hash, out var d))
        {
            throw new Exception("Moved note is not found in NoteMap!");
        }
        bool is_hit_note = HitnoteTypes.Contains(d.Type);
        NoteHash origin_hash = target_hash;
        d.Position = origin_hash.Position;
        d.Track = origin_hash.Track;
        NoteMap.Add(origin_hash, d);
        if (NoteMapBar.TryGetValue(target_pos.GetWhole(), out var list))
        {
            if (origin_hash.Position.GetWhole() == target_pos.GetWhole())
                list[list.FindIndex(n => n.Equals(moved_hash))] = origin_hash;
            else
            {
                list.Remove(moved_hash);
                if (NoteMapBar.TryGetValue(origin_hash.Position.GetWhole(), out var list1))
                    list1.Add(origin_hash);
                else
                {
                    List<NoteHash> new_list = new List<NoteHash>();
                    new_list.Add(origin_hash);
                    NoteMapBar.Add(origin_hash.Position.GetWhole(), new_list);
                }
            }
        }
        if (d.Type == NoteType.BPM)
        {
            BPMNoteList.Remove(moved_hash);
            BPMNoteList.Add(origin_hash);
        }
        if (CandidateNoteList.Contains(moved_hash))
        {
            CandidateNoteList[CandidateNoteList.FindIndex(n => n.Equals(moved_hash))] = origin_hash;
        }
        if (SelectedNoteList.Contains(moved_hash))
        {
            SelectedNoteList[SelectedNoteList.FindIndex(n => n.Equals(moved_hash))] = origin_hash;
        }
    }
}
public class ModifyHitNoteDataCommand : ICommand
{
    private Dictionary<NoteHash, bool> targets = new Dictionary<NoteHash, bool>();//Targets,If Successfully Modified
    private Dictionary<NoteHash, HitNoteData> origin_datas = new Dictionary<NoteHash, HitNoteData>();
    private int count = -1;
    private int remove_count = -1;
    private float scale = -1;
    private int y_offset = -1;
    private NoteType type = NoteType.Invalid;
    public ModifyHitNoteDataCommand(List<NoteHash> targets, int count = default, int remove_count = default, float scale = default,
        NoteType type = default, int y_offset = default)
    {
        foreach (var h in targets)
            this.targets.Add(h, true);
        this.count = count;
        this.remove_count = remove_count;
        this.scale = scale;
        this.type = type;
        this.y_offset = y_offset;
    }

    public string DisplayName => $"Modified {targets.Count} Notes.";
    public bool Execute()
    {
        bool ever_modified = false;
        foreach (var h in targets.Keys)
        {
            NoteMap.TryGetValue(h, out var d);
            if (!HitnoteTypes.Contains(d.Type))
            {
                targets[h] = false;
                continue;
            }

            if (count > 0)
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (HitNoteData)d.Clone());
                ((HitNoteData)d).Count = count;
                ever_modified = true;
            }
            if (remove_count > 0 && remove_count < ((HitNoteData)d).Count)
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (HitNoteData)d.Clone());
                ((HitNoteData)d).RemoveCount = remove_count;
                ever_modified = true;
            }
            if (scale >= 0)
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (HitNoteData)d.Clone());
                ((HitNoteData)d).Scale = scale;
                ever_modified = true;
            }
            if (HitnoteTypes.Contains(type))
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (HitNoteData)d.Clone());
                ((HitNoteData)d).Type = type;
                ever_modified = true;
            }
            if (y_offset >= 0)
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (HitNoteData)d.Clone());
                ((HitNoteData)d).YOffset = y_offset;
                ever_modified = true;
            }
        }
        if (!ever_modified) return false;
        else return true;
    }
    public void Undo()
    {
        foreach (var h in targets.Keys)
        {
            if (targets[h] == false) continue;
            if (origin_datas.TryGetValue(h, out var d))
            {
                NoteMap[h] = d.Clone();
            }
            else
                throw new Exception("Modified Hit Note not found in NoteMap!");
        }
    }
}
public class ModifyBPMNoteDataCommand : ICommand
{
    private Dictionary<NoteHash, bool> targets = new Dictionary<NoteHash, bool>();//Targets,If Successfully Modified
    private Dictionary<NoteHash, BPMNoteData> origin_datas = new Dictionary<NoteHash, BPMNoteData>();
    private float bpm_value = -1f;
    public ModifyBPMNoteDataCommand(List<NoteHash> targets, float bpm_value = default)
    {
        foreach (var h in targets)
            this.targets.Add(h, true);
        this.bpm_value = bpm_value;
    }

    public string DisplayName => $"Modified {targets.Count} Notes.";
    public bool Execute()
    {
        bool ever_modified = false;
        foreach (var h in targets.Keys)
        {
            NoteMap.TryGetValue(h, out var d);
            if (d.Type != NoteType.BPM)
            {
                targets[h] = false;
                continue;
            }

            if (bpm_value > 0)
            {
                if (!origin_datas.ContainsKey(h))
                    origin_datas.Add(h, (BPMNoteData)d.Clone());
                ((BPMNoteData)d).BPMValue = bpm_value;
                ever_modified = true;
            }
        }
        if (!ever_modified) return false;
        else return true;
    }
    public void Undo()
    {
        foreach (var h in targets.Keys)
        {
            if (targets[h] == false) continue;
            if (origin_datas.TryGetValue(h, out var d))
            {
                NoteMap[h] = d.Clone();
            }
            else
                throw new Exception("Modified Hit Note not found in NoteMap!");
        }
    }
}
public partial class CommandSystem
{
    private static Stack<ICommand> undo_stack = new(100);
    private static Stack<ICommand> redo_stack = new(50);

    public static bool ExecuteCommand(ICommand command)
    {
        if (command.Execute())
        {
            undo_stack.Push(command);
            redo_stack.Clear();
            Editor.Saved = false;
            return true;
        }
        return false;
    }
    public static void ClearStack()
    {
        undo_stack.Clear();
        redo_stack.Clear();
    }
    public static bool Undo()
    {
        if (undo_stack.Count <= 0) return false;
        Editor.Saved = false;
        ICommand command = undo_stack.Pop();
        command.Undo();
        redo_stack.Push(command);
        return true;
    }
    public static bool Redo()
    {
        if (redo_stack.Count <= 0) return false;
        Editor.Saved = false;
        ICommand command = redo_stack.Pop();
        command.Redo();
        undo_stack.Push(command);
        return true;
    }
}
