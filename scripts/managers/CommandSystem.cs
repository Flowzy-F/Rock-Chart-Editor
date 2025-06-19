using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static Utils;
using static NoteInfo;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Reflection;


#region Operator
//Operators are for performing tasks like update notedatas in NoteMap etc. when modifying notes.
//Operators also perform accompanying tasks like updating hashes in NoteMapBar.
//They also provide methods for extra opertion like adding notehash to the spcific list for a effect-type note.
public interface INoteOperator
{
    public bool Add(NoteData d);
    public bool Delete(NoteHash h);
    public bool MoveTo(NoteHash h, NoteHash newPos);
    public bool CheckIfPositionValid(NoteHash h);
    public enum ModifyOption;
}
public class NoteOperatorBase : INoteOperator
{
    public virtual bool Add(NoteData d)
    {
        if (!CheckIfPositionValid(d.GetNotehash())) return false;
        //Map
        NoteMap.Add(d.GetNotehash(), d.Clone());
        //Map Bar
        if (NoteMapBar.TryGetValue(d.Position.GetWhole(), out var list))
            list.Add(d.GetNotehash());
        else
        {
            List<NoteHash> new_list = new List<NoteHash>();
            new_list.Add(d.GetNotehash());
            NoteMapBar.Add(d.Position.GetWhole(), new_list);
        }
        return true;
    }
    public virtual bool Delete(NoteHash h)
    {
        if (!NoteMap.ContainsKey(h))
            throw new Exception("Target note was not found when deleting!Do not modify NoteMap outside CommandSystem!");
        //Map
        NoteMap.Remove(h);
        //Map Bar
        NoteMapBar.TryGetValue(h.Position.GetWhole(), out var list);
        list.Remove(h);
        if (list.Count <= 0)
        {
            NoteMapBar.Remove(h.Position.GetWhole());
        }
        if(SelectedNoteList.Remove(h))
            NoteEditManager.UpdateNoteEdits();//UNDONE:This should be called only once when deleting.
                                              //But when undoing a large Paste performance,this will be called for multiple times.
        CandidateNoteList.Remove(h);
        return true;
    }
    public virtual bool MoveTo(NoteHash h, NoteHash newPos)
    {
        if (!CheckIfPositionValid(newPos)) return false;
        if (!NoteMap.TryGetValue(h, out var d))
        {
            throw new Exception("Note is not found in NoteMap!");
        }
        d.Position = newPos.Position;
        //map
        NoteMap.Remove(h);
        NoteMap.Add(d.GetNotehash(), d);
        //map bar
        NoteMapBar.TryGetValue(h.Position.GetWhole(), out var list);
        list.Remove(h);
        if (newPos.Position.GetWhole() == h.Position.GetWhole())
        {
            list.Add(newPos);
        }
        else
        {
            if (list.Count <= 0)
            {
                NoteMapBar.Remove(h.Position.GetWhole());
            }
            if (NoteMapBar.TryGetValue(newPos.Position.GetWhole(), out var newPosList))
            {
                newPosList.Add(newPos);
            }
            else
            {
                List<NoteHash> new_list = new List<NoteHash>();
                new_list.Add(newPos);
                NoteMapBar.Add(newPos.Position.GetWhole(), new_list);
            }
        }
        //candidate&select
        if (CandidateNoteList.Contains(h))
        {
            CandidateNoteList[CandidateNoteList.FindIndex(n => n.Equals(h))] = newPos;
        }
        if (SelectedNoteList.Contains(h))
        {
            SelectedNoteList[SelectedNoteList.FindIndex(n => n.Equals(h))] = newPos;
        }
        NoteEditManager.UpdateNoteEdits();
        return true;
    }
    public virtual bool CheckIfPositionValid(NoteHash h)
    {
        return !NoteMap.ContainsKey(h) && h.Position >= Fraction.Zero();
    }
}
public class HitNoteOperator : NoteOperatorBase
{
    public override bool MoveTo(NoteHash h, NoteHash newPos)
    {
        if (!CheckIfPositionValid(newPos)) return false;
        //OperatorBase didn't modify the track.
        if (h.Track != newPos.Track)
        {
            if (!NoteMap.TryGetValue(h, out var d))
            {
                throw new Exception("Moving note is not found in NoteMap!");
            }
            ((HitNoteData)d).Track = newPos.Track;
        }
        return base.MoveTo(h, newPos);
    }
    public override bool CheckIfPositionValid(NoteHash h)
    {
        return base.CheckIfPositionValid(h) && h.Track > TrackLimitLeft && h.Track < TrackLimitRight;
    }
}
public class BPMNoteOperator : NoteOperatorBase
{
    public override bool Add(NoteData d)
    {
        if (!base.Add(d)) return false;
        BPMNoteList.Add(d.GetNotehash());
        return true;
    }
    public override bool Delete(NoteHash h)
    {
        if (!base.Delete(h)) return false;
        BPMNoteList.Remove(h);
        return true;
    }
    public override bool MoveTo(NoteHash h, NoteHash newPos)
    {
        if (!base.MoveTo(h, newPos)) return false;
        BPMNoteList.Remove(h);
        BPMNoteList.Add(newPos);
        SyncTimeSystem.BuildBPMTimeLine();
        return true;
    }
    public override bool CheckIfPositionValid(NoteHash h)
    {
        return !NoteMap.ContainsKey(h) && h.Position > Fraction.Zero() && h.Track == NoteHash.EFFECT_NOTE_TRACK;
    }
}
public class CameraNoteOperator : NoteOperatorBase
{
    public override bool Add(NoteData d)
    {
        if (!base.Add(d)) return false;
        CameraNoteList.Add(d.GetNotehash());
        return true;
    }
    public override bool Delete(NoteHash h)
    {
        if (!base.Delete(h)) return false;
        CameraNoteList.Remove(h);
        return true;
    }
    public override bool MoveTo(NoteHash h, NoteHash newPos)
    {
        if (!base.MoveTo(h, newPos)) return false;
        CameraNoteList.Remove(h);
        CameraNoteList.Add(newPos);
        return true;
    }
    public override bool CheckIfPositionValid(NoteHash h)
    {
        return base.CheckIfPositionValid(h) && h.Track == NoteHash.EFFECT_NOTE_TRACK;
    }
}
public class BKGNoteOperator : NoteOperatorBase
{
    public override bool Add(NoteData d)
    {
        if (!base.Add(d)) return false;
        BKGNoteList.Add(d.GetNotehash());
        return true;
    }
    public override bool Delete(NoteHash h)
    {
        if (!base.Delete(h)) return false;
        BKGNoteList.Remove(h);
        return true;
    }
    public override bool MoveTo(NoteHash h, NoteHash newPos)
    {
        if (!base.MoveTo(h, newPos)) return false;
        BKGNoteList.Remove(h);
        BKGNoteList.Add(newPos);
        return true;
    }
    public override bool CheckIfPositionValid(NoteHash h)
    {
        return base.CheckIfPositionValid(h) && h.Track == NoteHash.EFFECT_NOTE_TRACK;
    }
}
public static class OperatorFactory
{
    private static readonly Dictionary<NoteType, INoteOperator> operators = new Dictionary<NoteType, INoteOperator>()
    {
        [NoteType.Hit] = new HitNoteOperator(),
        [NoteType.BPM] = new BPMNoteOperator(),
        [NoteType.BKG] = new BKGNoteOperator(),
        [NoteType.Camera] = new CameraNoteOperator(),
        [NoteType.Invalid] = new NoteOperatorBase(),
    };
    public static INoteOperator GetOperator(NoteType type)
    {
        if (operators.TryGetValue(type, out INoteOperator op))
        {
            return op;
        }
        else
        {
            operators.TryGetValue(NoteType.Invalid, out var defaultOp);
            return defaultOp;
        }
    }
}
#endregion
#region ModificationRequest
public class ModificationRequest
{
    private readonly Dictionary<string, object> modifiedProperties = new Dictionary<string, object>();
    public ModificationRequest Set<T>(Expression<Func<NoteData, T>> propertyExpr, T value)
    {
        string name = GetPropertyName(propertyExpr);

        if (!modifiedProperties.ContainsKey(name))
        {
            modifiedProperties.Add(name, value);
        }
        else
            modifiedProperties[name] = value;
        return this;
    }
    public ModificationRequest SetFor<TNote, TValue>(Expression<Func<TNote, TValue>> propertyExpr, TValue value)
        where TNote : NoteData
    {
        string name = GetPropertyName(propertyExpr);

        if (!modifiedProperties.ContainsKey(name))
        {
            modifiedProperties.Add(name, value);
        }
        else
            modifiedProperties[name] = value;
        return this;
    }
    //Save properties of a certain type of NoteData
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> propertyCache = new();
    public bool ApplyTo(NoteData data)
    {
        var properties = propertyCache.GetOrAdd(data.GetType(),
            type => type.GetProperties()
            .Where(prop => prop.GetCustomAttribute<ModifiableAttribute>() != null)
            .ToDictionary(prop => prop.Name));
        bool modified = false;
        foreach (var (name, value) in modifiedProperties)
        {
            if (properties.TryGetValue(name, out PropertyInfo prop))
            {
                if (prop.GetValue(data) == value)//Not modified.
                    continue;
                prop.SetValue(data, value);

                if (prop.GetValue(data).Equals(value))//Successfully Modified
                {
                    modified = true;
                }
            }
        }
        return modified;
    }
}
#endregion

#region Commands
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
        if (target_datas.Any(target_data => target_data.Type != NoteType.Hit))
            throw new Exception("A HitNoteData's type is not Hitnote!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add((HitNoteData)d.Clone(), true);
        }
        this.add_count = add_count;
    }

    public bool Execute()
    {
        bool executed = false;//When there is a note on the target position and [!add_count],this command is actually not executed.
                              //This will be set to true when any note is added successfully.
        if (target_datas.Count <= 0) return false;
        if (target_datas.Keys.Any(target_data => target_data.Track < TrackLimitLeft || target_data.Track > TrackLimitRight)) return false;
        foreach (var d in target_datas.Keys)
        {
            if (NoteMap.TryGetValue(d.GetNotehash(), out NoteData data))
            {
                if (add_count && ((HitNoteData)data).Color == ((HitNoteData)d).Color)
                {
                    ((HitNoteData)data).Count += (d).Count;
                    executed = true;
                    continue;
                }
                else
                    target_datas[d] = false;
            }
            else
            {
                if (OperatorFactory.GetOperator(NoteType.Hit).Add(d))
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
                    if (((HitNoteData)d).Count > 0)
                        continue;
                }
                OperatorFactory.GetOperator(NoteType.Hit).Delete(delete_hash);
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
        foreach (var d in target_datas.Keys)
        {
            if (NoteMap.TryGetValue(new NoteHash(d.Position, NoteType.BPM), out NoteData data))
            {
                origin_bpm = ((BPMNoteData)data).BPMValue;
                ((BPMNoteData)data).BPMValue = d.BPMValue;
            }
            else
            {
                OperatorFactory.GetOperator(NoteType.BPM)?.Add(d);
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
            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                if (origin_bpm > 0)
                {
                    ((BPMNoteData)d).BPMValue = origin_bpm;
                    continue;
                }
                OperatorFactory.GetOperator(NoteType.BPM)?.Delete(delete_hash);
            }
        }
        if (rebuild_bpm_timeline) SyncTimeSystem.BuildBPMTimeLine();
    }
}
public class AddBKGNoteCommand : ICommand
{
    private Dictionary<BKGNoteData, bool> target_datas = new Dictionary<BKGNoteData, bool>();//Target Data,If Successfully Added
    public string DisplayName => $"Added {target_datas.Count} BKG Note(s).";
    public AddBKGNoteCommand(List<BKGNoteData> target_datas)
    {
        if (target_datas.Any(target_data => target_data.Type != NoteType.BKG))
            throw new Exception("A BKGNoteData's type is not NoteType.BKG!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add((BKGNoteData)d.Clone(), true);
        }
    }

    public bool Execute()
    {
        bool executed = false;//When there is a note on the target position ,this command is actually not executed.
                              //This will be set to true when any note is added successfully.
        if (target_datas.Count <= 0) return false;
        foreach (var d in target_datas.Keys)
        {
            if (NoteMap.ContainsKey(d.GetNotehash()))
            {
                target_datas[d] = false;
                continue;
            }
            OperatorFactory.GetOperator(NoteType.BKG)?.Add(d);
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
            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                OperatorFactory.GetOperator(NoteType.BKG)?.Delete(delete_hash);
            }
        }
    }
}
public class AddCameraNoteCommand : ICommand
{
    private Dictionary<CameraNoteData, bool> target_datas = new Dictionary<CameraNoteData, bool>();//Target Data,If Successfully Added
    public string DisplayName => $"Added {target_datas.Count} Camera Note(s).";
    public AddCameraNoteCommand(List<CameraNoteData> target_datas)
    {
        if (target_datas.Any(target_data => target_data.Type != NoteType.Camera))
            throw new Exception("A BPMNoteData's type is not NoteType.BPM!");
        foreach (var d in target_datas)
        {
            this.target_datas.Add((CameraNoteData)d.Clone(), true);
        }
    }

    public bool Execute()
    {
        if (target_datas.Count <= 0) return false;
        bool execute = false;
        foreach (var d in target_datas.Keys)
        {
            if (OperatorFactory.GetOperator(NoteType.Camera).Add(d))
                execute = true;
        }
        return execute;
    }
    public void Undo()
    {
        foreach (var target_data in target_datas.Keys)
        {
            if (target_datas[target_data] == false) continue;

            NoteHash delete_hash = target_data.GetNotehash();
            if (NoteMap.TryGetValue(delete_hash, out var d))
            {
                OperatorFactory.GetOperator(NoteType.Camera)?.Delete(delete_hash);
            }
        }
    }
}
public class AddNoteCommand : ICommand
{
    private List<ICommand> commands = new List<ICommand>();

    private List<HitNoteData> hit_notes = new List<HitNoteData>();
    private List<BPMNoteData> bpm_notes = new List<BPMNoteData>();
    private List<BKGNoteData> bkg_notes = new List<BKGNoteData>();
    private List<CameraNoteData> camera_notes = new List<CameraNoteData>();
    public string DisplayName => $"Added {hit_notes.Count + bpm_notes.Count + camera_notes.Count} Note(s).";
    public AddNoteCommand(List<NoteData> target_datas, bool add_count = true, bool rebuild_bpm_timeline = false)
    {
        //Init commands
        foreach (var d in target_datas.Where(n => n.Type == NoteType.Hit).ToList())
        {
            hit_notes.Add((HitNoteData)d);
        }
        foreach (var d in target_datas.Where(n => n.Type == NoteType.BPM).ToList())
        {
            bpm_notes.Add((BPMNoteData)d);
        }
        foreach (var d in target_datas.Where(n => n.Type == NoteType.BKG).ToList())
        {
            bkg_notes.Add((BKGNoteData)d);
        }
        foreach (var d in target_datas.Where(n => n.Type == NoteType.Camera).ToList())
        {
            camera_notes.Add((CameraNoteData)d);
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
        if (camera_notes.Count > 0)
        {
            AddCameraNoteCommand cmd = new AddCameraNoteCommand(camera_notes);
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

        this.count = count;
    }
    public bool Execute()
    {
        foreach (var h in targets.Keys)
        {
            if (NoteMap.TryGetValue(h, out var d))
            {
                OperatorFactory.GetOperator(d.Type)?.Delete(h);
            }
            else
                throw new Exception("Deleting note isn't found in NoteMap!");

            if (count > 0 && d.Type == NoteType.Hit)
            {
                ((HitNoteData)d).Count -= count;
                if (((HitNoteData)d).Count > 0) continue;
            }

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
                if (count > 0 && d.Type == NoteType.Hit)
                {
                    ((HitNoteData)d).Count += count;
                    continue;
                }
                else
                    throw new Exception("Deleted note's position has been occupied when undoing!");
            }
            OperatorFactory.GetOperator(p.Key.NoteType)?.Add(p.Value);
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
            NoteHash new_hash = new NoteHash(h.Position + pos_addition, h.NoteType, h.Track + track_addition);//Track addition will be

            if (OperatorFactory.GetOperator(h.NoteType).MoveTo(h, new_hash))
            {
                target_notes[h] = true;
                moved_hashes.Add(h, new_hash);
                moved = true;
            }
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
            ordered_hashes = moved_hashes.Keys.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator);
        else
            ordered_hashes = moved_hashes.Keys.OrderByDescending(n => (float)n.Position.Numerator / n.Position.Denominator);
        if (track_addition >= 0)
            ordered_hashes = ordered_hashes.ThenBy(n => n.Track);
        else
            ordered_hashes = ordered_hashes.ThenByDescending(n => n.Track);
        foreach (var h in ordered_hashes)
        {
            if (target_notes[h] == false) continue;
            OperatorFactory.GetOperator(h.NoteType).MoveTo(moved_hashes[h], h);
            moved_hashes.Remove(h);
        }
    }
}
public class MoveNoteToCommand : ICommand
{
    private NoteHash target_hash;//Target Hash,If Successfully Moved
    private NoteHash to;
    public string DisplayName => $"Moved a note to:Track {to.Track},{to.Position}.";
    public MoveNoteToCommand(NoteHash target_hash,
         NoteHash to)
    {
        this.target_hash = target_hash;
        this.to = to;
    }
    public bool Execute()
    {
        return OperatorFactory.GetOperator(target_hash.NoteType).MoveTo(target_hash, to);
    }
    public void Undo()
    {
        OperatorFactory.GetOperator(to.NoteType)?.MoveTo(to, target_hash);
    }
}
public class ModifyNoteDataCommand : ICommand
{
    private Dictionary<NoteHash, bool> targets = new Dictionary<NoteHash, bool>();//Targets,If Successfully Modified
    private Dictionary<NoteHash, NoteData> origin_datas = new Dictionary<NoteHash, NoteData>();
    private ModificationRequest request;
    public ModifyNoteDataCommand(List<NoteHash> targets, ModificationRequest request)
    {
        foreach (var h in targets)
        {
            if (!NoteMap.TryGetValue(h, out var d))
                throw new Exception("Note not found in NoteMap!");
            this.targets.Add(h, true);
            this.origin_datas.Add(h, d.Clone());
        }

        this.request = request;
    }

    public string DisplayName => $"Modified {targets.Count} Notes.";
    public bool Execute()
    {
        bool ever_modified = false;
        foreach (var h in targets.Keys)
        {
            NoteMap.TryGetValue(h, out var d);
            if (request.ApplyTo(d))
                ever_modified = true;
            else
                targets[h] = false;
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
#endregion