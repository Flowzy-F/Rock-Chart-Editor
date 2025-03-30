using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Utils;
public class NoteInfo
{
    public static readonly NoteType[] HitnoteTypes = { NoteType.Normal, NoteType.Gold };

    public enum NoteType
    {
        Normal,
        Gold,
        BKG,
        BPM,
        Invalid,
    }
    public class NoteData
    {
        protected NoteType type;
        public virtual NoteType Type { get { return type; } set { type = value; } }
        public Fraction Position;
        public float Track;
        public NoteData(NoteType type, Fraction position, float track)
        {
            this.Type = type;
            this.Position = position;
            this.Track = track;
        }
        public NoteHash GetNotehash()
        {
            return new NoteHash(Position, Track);
        }

        public virtual NoteData Clone()
        {
            return new NoteData(Type, Position, Track);
        }
    }
    public class HitNoteData : NoteData
    {
        private int count = 1;
        private int remove_count = 0;
        private float scale = 1;
        private int y_offset = 0;

        public override NoteType Type { get => base.Type; set  
            {
                if (!HitnoteTypes.Contains(value)) throw new Exception("Hit note type is wrong!");
                base.Type = value; 
            }}
        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                if (count < 0) count = 0;
                if (remove_count >= count) remove_count = (count - 1 <= 0) ? 0 : (count - 1);
            }
        }
        public int RemoveCount
        {
            get { return remove_count; }
            set
            {
                remove_count = value;
                if (remove_count < 0) remove_count = 0;
                if (remove_count >= count) remove_count = (count - 1 <= 0) ? 0 : (count - 1);
            }
        }
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value > 0 ? value : scale;
            }
        }
        public int YOffset
        {
            get { return y_offset; }
            set { y_offset= value<0?0:value; }
        }
        public HitNoteData(NoteType type, Fraction position, float track,int count,int remove_count,float scale=1.0f,int y_offset=0)
            : base(type, position, track)
        {
            if (!HitnoteTypes.Contains(type)) throw new Exception("Hit note type is wrong!");
            this.Scale= scale;
            this.Count=count;
            this.YOffset = y_offset;
            this.RemoveCount = remove_count;
        }
        public override NoteData Clone()
        {
            return new HitNoteData(Type,Position,Track,count,remove_count);
        }
    }
    public class BPMNoteData : NoteData
    {
        private float bpm_value;
        public float BPMValue
        {
            get { return bpm_value; }
            set { bpm_value = value <= 0 ? 1 : value; }
        }
        public override NoteType Type
        {
            get => base.Type; set
            {
                if (value!=NoteType.BPM) throw new Exception("BPM note type is wrong!");
                base.Type = value;
            }
        }
        public BPMNoteData(NoteType type, Fraction position, float track,float bpm_value) : base(type, position, track)
        {
            if (type!=NoteType.BPM ) throw new Exception("BPM note type is wrong!");
            BPMValue = bpm_value;
        }
        public override NoteData Clone()
        {
            return new BPMNoteData(Type, Position, Track, bpm_value);
        }
    }

    public readonly struct NoteHash:IEquatable<NoteHash>
    {
        public NoteHash(Fraction position,float track) 
        {
            this.Position = position;
            this.Track = track;
        }
        public Fraction Position { get; }
        public float Track { get; }
        public bool Equals(NoteHash other)
        {
            return Position.Reduced()==other.Position.Reduced()&&Track==other.Track;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine<float, int, int>(Track, Position.Reduced().Numerator, Position.Reduced().Denominator);
        }
    }
}
