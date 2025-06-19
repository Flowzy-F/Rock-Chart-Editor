using Godot;
using System;
using static Utils;
public class NoteInfo
{

    public enum NoteType
    {
        Hit,
        BKG,
        BPM,
        Camera,
        Invalid,
    }
    public enum HitColor
    {
        Normal,
        Gold,
        Performance,
        Invalid,
    }
    //Mark properties that could be modified,get detected when modify note.
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ModifiableAttribute : Attribute
    {
        string fieldName = null;
        public ModifiableAttribute(string fieldName) => this.fieldName = fieldName;
    }
    public enum EasingType
    {
        Linear = 0,
        InSine = 1,
        OutSine = 2,
        InOutSine = 3,
        InQuad = 4,
        OutQuad = 5,
        InOutQuad = 6,
        InCubic = 7,
        OutCubic = 8,
        InOutCubic = 9,
        InQuart = 10,
        OutQuart = 11,
        InOutQuart = 12,
        InQuint = 13,
        OutQuint = 14,
        InOutQuint = 15,
        InExpo = 16,
        OutExpo = 17,
        InOutExpo = 18,
        InCirc = 19,
        OutCirc = 20,
        InOutCirc = 21,
        InElastic = 22,
        OutElastic = 23,
        InOutElastic = 24,
        InBack = 25,
        OutBack = 26,
        InOutBack = 27,
        InBounce = 28,
        OutBounce = 29,
        InOutBounce = 30,
        Flash = 31,
        InFlash = 32,
        OutFlash = 33,
        InOutFlash = 34,
        NumEasingType,
    }
    public class NoteData
    {
        protected NoteType type = 0;
        public virtual NoteType Type { get { return type; } }
        public Fraction Position { get; set; } = new Fraction(0, 1);
        public NoteData() { }
        public NoteData(Fraction position)
        {
            this.Position = position;
        }
        public virtual NoteHash GetNotehash()
        {
            return new NoteHash(Position, Type);
        }

        public virtual NoteData Clone()
        {
            NoteData clone = new NoteData(Position);
            clone.type = type;
            return clone;
        }
    }
    public class HitNoteData : NoteData
    {
        private float track = 0;
        private int count = 1;
        private int remove_count = 0;
        private float scale = 1;
        private int y_offset = 0;

        public float Track
        {
            get { return track; }
            set
            {
                if (value < Editor.TrackLimitLeft) track = Editor.TrackLimitLeft;
                else if (value > Editor.TrackLimitRight) track = Editor.TrackLimitRight;
                else track = value;
            }
        }

        [Modifiable("color")]
        public HitColor Color { get; set; }
        [Modifiable("count")]
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
        [Modifiable("remove_count")]
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
        [Modifiable("scale")]
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value > 0 ? value : scale;
            }
        }
        [Modifiable("y_offset")]
        public int YOffset
        {
            get { return y_offset; }
            set { y_offset = value < 0 ? 0 : value; }
        }
        public HitNoteData() { this.type = NoteType.Hit; }
        public HitNoteData(Fraction position, HitColor color, float track, int count, int remove_count, float scale = 1.0f, int y_offset = 0)
        {
            this.type = NoteType.Hit;
            this.Position = position;
            this.Track = track;
            this.Color = color;
            this.Scale = scale;
            this.Count = count;
            this.YOffset = y_offset;
            this.RemoveCount = remove_count;
        }
        public override NoteData Clone()
        {
            HitNoteData clone = new HitNoteData();
            clone.Position = this.Position;
            clone.Track = this.Track;
            clone.Color = this.Color;
            clone.Scale = this.scale;
            clone.Count = this.count;
            clone.RemoveCount = this.remove_count;
            clone.YOffset = this.y_offset;
            return clone;
        }
        public override NoteHash GetNotehash()
        {
            return new NoteHash(Position, Type, Track);
        }
    }
    public class BPMNoteData : NoteData
    {
        private float bpm_value = 120;
        [Modifiable("bpm_value")]
        public float BPMValue
        {
            get { return bpm_value; }
            set { bpm_value = value <= 0 ? 1 : value; }
        }
        public BPMNoteData() { this.type = NoteType.BPM; }
        public BPMNoteData(Fraction position, float bpm_value)
        {
            type = NoteType.BPM;
            this.Position = position;
            BPMValue = bpm_value;
        }
        public override NoteData Clone()
        {
            BPMNoteData clone = new BPMNoteData();
            clone.bpm_value = this.bpm_value;
            clone.Position = this.Position;
            return clone;
        }
    }
    public class CameraNoteData : NoteData
    {
        private Fraction duration = Fraction.One();
        [Modifiable("duration")]
        public Fraction Duration
        {
            get { return duration; }
            set { if (value >= Fraction.Zero()) duration = value; }
        }
        [Modifiable("easing")]
        public EasingType Easing { get; set; } = EasingType.Linear;
        [Modifiable("position_movement_x")]
        public float PositionMovementX { get; set; } = 0;
        [Modifiable("position_movement_y")]
        public float PositionMovementY { get; set; } = 0;
        [Modifiable("position_movement_z")]
        public float PositionMovementZ { get; set; } = 0;
        [Modifiable("rotation_movement_x")]
        public float RotationMovementX { get; set; } = 0;
        [Modifiable("rotation_movement_y")]
        public float RotationMovementY { get; set; } = 0;
        [Modifiable("rotation_movement_z")]
        public float RotationMovementZ { get; set; } = 0;
        public CameraNoteData() { this.type = NoteType.Camera; }
        public CameraNoteData(Fraction begin, Fraction duration, EasingType easing, Vector3 positionMovement, Vector3 rotationMovementAngle)
        {
            this.type = NoteType.Camera;
            this.Position = begin;
            this.duration = duration;
            this.Easing = easing;
            this.PositionMovementX = positionMovement.X;
            this.PositionMovementY = positionMovement.Y;
            this.PositionMovementZ = positionMovement.Z;
            this.RotationMovementX = rotationMovementAngle.X;
            this.RotationMovementY = rotationMovementAngle.Y;
            this.RotationMovementZ = rotationMovementAngle.Z;
        }
        public override NoteData Clone()
        {
            CameraNoteData clone = new CameraNoteData();
            clone.Position = this.Position;
            clone.Duration = this.duration;
            clone.Easing = this.Easing;
            clone.PositionMovementX = this.PositionMovementX;
            clone.PositionMovementY = this.PositionMovementY;
            clone.PositionMovementZ = this.PositionMovementZ;
            clone.RotationMovementX = this.RotationMovementX;
            clone.RotationMovementY = this.RotationMovementY;
            clone.RotationMovementZ = this.RotationMovementZ;

            return clone;
        }
    }
    public class BKGNoteData : NoteData
    {
        public BKGNoteData() { this.type = NoteType.BKG; }
        public BKGNoteData(Fraction position)
        {
            type = NoteType.BKG;
            this.Position = position;
        }
        public override NoteData Clone()
        {
            BKGNoteData clone = new BKGNoteData();
            clone.Position = this.Position;
            return clone;
        }
    }

    public readonly struct NoteHash : IEquatable<NoteHash>
    {
        public const int EFFECT_NOTE_TRACK = 99;
        public NoteHash(Fraction position, NoteType noteType, float track)
        {
            this.Position = position;
            this.NoteType = noteType;
            this.Track = track;
        }
        //For effect note type(types except HitNote).
        public NoteHash(Fraction position, NoteType noteType)
        {
            this.Position = position;
            this.NoteType = noteType;
            this.Track = EFFECT_NOTE_TRACK;
        }
        public Fraction Position { get; }
        public NoteType NoteType { get; }
        public float Track { get; } = 0;
        public bool Equals(NoteHash other)
        {
            return Position.Reduced() == other.Position.Reduced() && NoteType == other.NoteType && Track == other.Track;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine<int, int, int>(Position.Reduced().Numerator, Position.Reduced().Denominator, (int)NoteType);
        }
    }
}
