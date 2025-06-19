using Godot;
using System;
using static NoteInfo;
using static Utils;
using System.Collections.Generic;
using System.Linq;
public enum Difficulty
{
    None,
    Fun,
    Easy,
    Normal,
    Hard,
    Powerful,
    SoPowerful,
}
public partial class Editor : Node
{

    #region Objects
    public DrawerGroup DrawerGroup;
    public Control GridDrawer;
    public Control NoteDrawer;
    public Control CursorDrawer;
    public AudioStreamPlayer BGMPlayer;
    public AudioStreamPlayer SEPlayerNormal;
    public AudioStreamPlayer SEPlayerGold;
    public VideoStreamPlayer BGAPlayer;
    public Dictionary<NoteType, PanelContainer> NoteEdits = new Dictionary<NoteType, PanelContainer>();
    public TipManager TipManager;
    public FileDialog LoadDialog;
    public FileDialog SaveDialog;
    public Label EffectTrackLabel;
    #endregion
    #region META
    private static string title = "Title";
    public static string Title
    {
        get { return title; }
        set { title = value; }
    }
    public static string artist = "Artist";
    public static string Artist
    {
        get { return artist; }
        set { artist = value; }
    }
    private static string mapper = "Mapper";
    public static string Mapper
    {
        get { return mapper; }
        set { mapper = value; }
    }
    private static int mass = 0;
    public static int Mass
    {
        get
        {
            return mass;
        }
        set
        {
            if (value <= 0) mass = 0;
            else if (value >= 99999) mass = 99999;
            else mass = value;
        }
    }
    private static Difficulty diff = 0;
    public static Difficulty Diff
    {
        get { return diff; }
        set { diff = value; }
    }
    private static float bpm = 128;
    public static float BPM
    {
        get { return bpm; }
        set 
        { 
            bpm = value > 0 ? value : bpm;
            BottomPosition = -SyncTimeSystem.ToBarPosition(offset);
        }
    }
    private static float offset;
    public static float Offset
    {
        get { return offset; }
        set
        {
            offset = value;
            BottomPosition = -SyncTimeSystem.ToBarPosition(offset);
        }
    }
    private static float bg_offset = 0;
    public static float BGOffset
    {
        get { return bg_offset; }
        set { bg_offset = value; }
    }
    private static string bgm_path = "";
    public static string BGMPath
    {
        get { return bgm_path; }
        set { bgm_path = value; }
    }
    private static string bga_path = "";
    public static string BGAPath
    {
        get { return bga_path; }
        set { bga_path = value; }
    }

    #endregion

    #region Grid
    public static NoteType DisplayedEffectNoteType = NoteType.Camera;
    public static readonly int TrackCount = 4;
    public static readonly float BeatHeight = 200;
    public static readonly float ZoomMin = 0.5f;
    public static readonly float ZoomMax = 6f;
    private static float zoom = 1.0f;
    public static float Zoom
    {
        get { return zoom; }
        set { zoom = value; }
    }
    public static readonly Vector2 DrawerPosition = new Vector2(350, 75);
    public static readonly Vector2 DrawerSize = new Vector2(350, 520);
    public static readonly Vector2 DrawerScale = new Vector2(1, 1);
    public static readonly Vector2 DrawerDisplaySize = DrawerSize * DrawerScale;
    public static readonly float NoteXOffset = 0.5f;//[0,1],distance to the left VLine.
    #endregion

    #region UI
    public Font DefaultFont;
    public static readonly float NoteSize = 7.5f;
    public static readonly float CursorSize = 4f;
    public static readonly float TrackLimitLeft = -4.0f;
    public static readonly float TrackLimitRight = 8.0f;
    public static readonly Color NormalLineColor = new Color(0, 255, 0);
    public static readonly Color BarLineColor = new Color(255, 0, 0);
    public static readonly Color SelectedLineColor = new Color(0, 0, 255);
    public static readonly Color NormalNoteColor = new Color(255, 255, 255);
    public static readonly Color GoldNoteColor = new Color(255, 215, 0);
    public static readonly Color PerformanceNoteColor = new Color(255, 0, 200);
    public static readonly Color BPMNoteColor = new Color(0, 0, 0);
    public static readonly Color BKGNoteColor = new Color(0, 0, 255);
    public static readonly Color CameraNoteColor = new Color(255, 0, 0);
    public static readonly Color SelectedNoteColor = new Color(255, 0, 0);
    public static readonly Color CandidateNoteColor = new Color(0, 255, 255);
    public static readonly Color MultipleSelectionRegionColor = new Color(255, 195, 0, 0.5f);
    #endregion

    #region Edit
    public static bool Saved { get; set; } = true;
    public static string SavePath { get; set; }
    private static int denominator = 4;
    public static int Denominator
    {
        get { return denominator; }
        set { denominator = value; Numerator = (int)Math.Floor(TimeOffset / (1.0f / Denominator)); }
    }
    private static int numerator = 0;
    public static int Numerator
    {
        get { return numerator; }
        set { numerator = value; }
    }
    private static int bar = 0;
    public static int Bar
    {
        get { return bar; }
        set { bar = value; }
    }
    private static float time_offset = 0;
    public static float TimeOffset
    {
        get { return time_offset; }
        set { time_offset = value; Numerator = (int)Math.Floor(TimeOffset / (1.0f / Denominator)); }
    }//[0,1), the offset of a bar.When you are at 3/4 of the bar,the TimeOffset will be 0.75f.
    #endregion


    public int MousePointedBar = 0;//Updated in DrawerGroup.cs
    public int MousePointedNumerator = 0;//Updated in DrawerGroup.cs
    public int MousePointedTrack = 0;//Updated in DrawerGroup.cs
    public Vector2I MousePointedLocalCoord = new Vector2I();//Updated in DrawerGroup.cs
    public Fraction SelectedBarPosition = new Fraction(0, 1);//For pasting notes

    #region Datas
    private static Dictionary<NoteHash, NoteData> note_map = new Dictionary<NoteHash, NoteData>();
    public static Dictionary<NoteHash, NoteData> NoteMap { get { return note_map; } }
    private static Dictionary<int, List<NoteHash>> note_map_bar = new Dictionary<int, List<NoteHash>>();//Tells notes in each bar.
    public static Dictionary<int, List<NoteHash>> NoteMapBar { get { return note_map_bar; } }
    private static List<NoteHash> bpm_note_list = new List<NoteHash>();
    public static List<NoteHash> BPMNoteList { get { return bpm_note_list; } }
    private static List<NoteHash> camera_note_list = new List<NoteHash>();
    public static List<NoteHash> CameraNoteList { get { return camera_note_list; } }
    private static List<NoteHash> bkg_note_list = new List<NoteHash>();
    public static List<NoteHash> BKGNoteList { get { return bkg_note_list; } }
    private static List<NoteHash> selected_note_list = new List<NoteHash>();
    public static List<NoteHash> SelectedNoteList { get { return selected_note_list; } }
    private static List<NoteHash> candidate_note_list = new List<NoteHash>();
    public static List<NoteHash> CandidateNoteList { get { return candidate_note_list; } }
    private static List<NoteData> note_clipboard = new List<NoteData>();
    public static List<NoteData> NoteClipboard
    {
        get { return note_clipboard; }
        set { note_clipboard = value.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator).ToList(); ; }
    }
    #endregion

    #region Other
    // Because of the feature of LineEdit in Godot,they can't lose focus until user click another UI Control.
    // This will be updated in InputManager.cs,in order to make LineEdits lose focus when <Enter> pressed or clicking outside the LineEdit.
    public List<LineEdit> FocusChangeNeedingLineEdits = new List<LineEdit>();
    public Label DebugLabel;
    public Vector2 MultipleSelectionStartPos = new Vector2();
    public Rect2 MultipleSelectionRegionRect = new Rect2();
    public Vector2 MultipleSelectionMouseOffset = new Vector2();
    public Vector2I MultipleSelectionCoord_Global = new Vector2I();
    private static float bottom_position = 0;
    public static float BottomPosition { get { return bottom_position; } set { bottom_position = value; } }
    public static bool is_multiple_selecting = false;
    public static bool IsMultipleSelecting { get { return is_multiple_selecting; } set { is_multiple_selecting = value; } }
    #endregion

    public static Editor Instance { get; private set; }

    #region Editing Process
    /// <param name="add_count">If <code>true</code> and there is a note at the target position,add count to this note.Otherwise return.</param>
    public bool PlaceHitNote(HitColor color, Fraction position, float track, int count, int remove_count, bool add_count, float scale = 1f, int y_offset = 0)
    {
        HitNoteData d = new HitNoteData(position, color, track, count, remove_count, scale, y_offset);
        ICommand cmd = new AddHitNoteCommand([d], add_count);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool PlaceBPMNoteOrModify(Fraction position, float bpm_value, bool rebuild_bpm_time_line = true)
    {
        BPMNoteData d = new BPMNoteData(position, bpm_value);
        ICommand cmd = new AddBPMNoteCommand([d], rebuild_bpm_time_line);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool PlaceBKGNote(Fraction position)
    {
        BKGNoteData d = new BKGNoteData(position);
        ICommand cmd = new AddBKGNoteCommand([d]);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool PlaceCameraNote(Fraction begin, Fraction duration, EasingType easing, Vector3 positionMovement, Vector3 rotationMovement)
    {
        CameraNoteData d = new CameraNoteData(begin, duration, easing, positionMovement, rotationMovement);
        ICommand cmd = new AddCameraNoteCommand([d]);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool ModifyNoteData(List<NoteHash> targets, ModificationRequest request)
    {
        ModifyNoteDataCommand cmd = new ModifyNoteDataCommand(targets, request);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    /// <summary>
    /// No BPM Timeline rebuilding.
    /// </summary>
    public bool MoveNote(List<NoteHash> hashes, Fraction position_addition, int track_addition)
    {
        MoveNoteCommand cmd = new MoveNoteCommand(hashes, position_addition, track_addition);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    /// <summary>
    /// No BPM Timeline rebuilding.
    /// </summary>
    public bool MoveNoteTo(NoteHash h, NoteHash to)
    {
        MoveNoteToCommand cmd = new MoveNoteToCommand(h, to);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    ///<summary>SelectedNoteList and CandidateNoteList are not cleared here.Please clear them manually.</summary>
    /// <param name="count">Works when deleted note is a hitnote.if <code>count <=0 </code>,directly delete the note. Otherwise split a count of the hitnote.</param>
    public bool DeleteNote(NoteHash hash, int count = -1, bool rebuild_bpm_timeline = true)
    {
        DeleteNotesCommand cmd = new DeleteNotesCommand([hash], rebuild_bpm_timeline, count);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    /// <summary>
    /// This cannot be redone.
    /// </summary>
    public void ClearNotes()
    {
        note_map.Clear();
        note_map_bar.Clear();
        bpm_note_list.Clear();
        selected_note_list.Clear();
        candidate_note_list.Clear();
        CommandSystem.ClearStack();
        SyncTimeSystem.BuildBPMTimeLine();
    }
    /// <returns>false if no notes were selected.</returns>
    public bool SelectCandidateNotes()
    {
        if (CandidateNoteList.Count <= 0) return false;
        foreach (NoteHash h in CandidateNoteList)
        {
            SelectNote(h);
        }
        CandidateNoteList.Clear();
        return true;
    }
    public void SelectNote(params NoteHash[] h)
    {
        foreach (NoteHash hash in h)
        {
            if (selected_note_list.Contains(hash)) continue;
            selected_note_list.Add(hash);
        }
    }
    public void RemoveSelectedNotes(params NoteHash[] h)
    {
        foreach (NoteHash hash in h)
        {
            selected_note_list.Remove(hash);
        }
    }
    public void ClearSelectedNotes()
    {
        SelectedNoteList.Clear();
    }
    public void AddCandidateNote(params NoteHash[] h)
    {
        foreach (NoteHash hash in h)
        {
            if (candidate_note_list.Contains(hash)) continue;
            candidate_note_list.Add(hash);
        }
    }
    public void ClearCandidateNotes()
    {
        candidate_note_list.Clear();
    }
    public bool DeleteSelectedNotes()
    {
        DeleteNotesCommand cmd = new DeleteNotesCommand(selected_note_list, true);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool Paste()
    {
        if (NoteClipboard.Count <= 0) return false;
        List<NoteData> fixed_clipboard = new List<NoteData>();
        foreach (var d in NoteClipboard)
        {
            NoteData fixed_d = d.Clone();
            fixed_d.Position = fixed_d.Position - NoteClipboard[0].Position + SelectedBarPosition;
            fixed_clipboard.Add(fixed_d);
        }
        AddNoteCommand cmd = new AddNoteCommand(fixed_clipboard, false, true);
        return (CommandSystem.ExecuteCommand(cmd));
    }
    public bool SelectDisplayedEffectNoteType(NoteType type)
    {
        if (type == DisplayedEffectNoteType) return false;
        switch (type)
        {
            case NoteType.BPM:
                EffectTrackLabel.Text = "BPM";
                break;
            case NoteType.BKG:
                EffectTrackLabel.Text = "BKG";
                break;
            case NoteType.Camera:
                EffectTrackLabel.Text = "Camera";
                break;
            default:
                return false;
        }
        DisplayedEffectNoteType = type;

        if (selected_note_list.RemoveAll(x => x.NoteType != NoteType.Hit) > 0)//Remove selected effect note when changing displayed type. 
            NoteEditManager.UpdateNoteEdits();
        NoteDrawer.QueueRedraw();
        return true;
    }
    #endregion
    #region Coord Trans
    /// <returns>A coord with the left-bottom as origin of the current visible grid.</returns>
    public static Vector2I ToLocalCoord(Vector2 pos)
    {
        int x = (int)Math.Floor((pos.X - DrawerPosition.X) / (DrawerDisplaySize.X / TrackCount));
        float fixed_pos_y = (DrawerDisplaySize.Y + DrawerPosition.Y + Editor.Instance.DrawerGroup.HeightOffset) - pos.Y;
        int y = (int)Math.Round(fixed_pos_y / (BeatHeight / Denominator / Zoom));
        return new Vector2I(x, y);
    }
    /// <returns>A coord with the left-bottom as origin of the grid.</returns>
    public static Vector2I ToGlobalCoord(Vector2 pos)
    {
        int x = (int)Math.Floor((pos.X - DrawerPosition.X) / (DrawerDisplaySize.X / TrackCount));
        float fixed_pos_y = (DrawerDisplaySize.Y + DrawerPosition.Y + Editor.Instance.DrawerGroup.HeightOffset) - pos.Y;
        int y = (int)Math.Round(fixed_pos_y / (BeatHeight / Denominator / Zoom));
        y += Denominator * Bar + Numerator;
        return new Vector2I(x, y);
    }
    public static Vector2 ToLocalPosition(Vector2I local_coord)
    {
        float x = (local_coord.X + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
        float y = BeatHeight / Denominator / Zoom * local_coord.Y
            - Editor.Instance.DrawerGroup.HeightOffset;
        return new Vector2(x, DrawerDisplaySize.Y - y);
    }
    public static Vector2 ToGlobalPosition(Vector2I global_coord)
    {
        float x = (global_coord.X + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
        float y = BeatHeight / Denominator / Zoom * (global_coord.Y - (Denominator * Bar + Numerator))
            - Editor.Instance.DrawerGroup.HeightOffset;
        return new Vector2(x, DrawerDisplaySize.Y - y);
    }
    #endregion
    public override void _Process(double delta)
    {
        if (MediaManager.IsPlaying)
        {
            double bar_pos = TimeOffset + Bar;
            //This is weird.I don't know why I write this.
            //If I Remove any of these two,bar_pos will return to the beginning when the BPMPlayer finishes playing.
            if (MediaManager.IsFinished || MediaManager.GetPlaybackPosition() < 0)
            {
                if (bar_pos < 0)
                    bar_pos = bar_pos+SyncTimeSystem.ToBarPosition((float)delta * BGMPlayer.PitchScale);
                else
                    bar_pos = SyncTimeSystem.ToBarPosition(SyncTimeSystem.ToTime((float)(bar_pos)) + (float)delta*BGMPlayer.PitchScale);
            }
            else
            {
                bar_pos = SyncTimeSystem.ToBarPosition(MediaManager.GetPlaybackPosition() - Offset);
                //f GD.Print(bar_pos);
            }
            Bar = (int)Math.Floor(bar_pos);
            TimeOffset = (float)(bar_pos - Bar);
            NoteTriggeringManager.CheckNotes();
            DrawerGroup.UpdateHeightOffset();
            DrawerGroup.RedrawAll();
            //DebugLabel.Text = $"小节:{Bar}\n小节进度:{TimeOffset}";
        }
        base._Process(delta);
    }
    public override void _Ready()
    {
        Instance = this;
        DebugLabel = GetTree().Root.GetNode<Label>("MainScene/DEBUG_LABEL");
        TipManager = GetTree().Root.GetNode<TipManager>("MainScene/Managers/TipManager");
        #region Font
        DefaultFont = ResourceLoader.Load<FontVariation>("res://fonts//default_font.tres");
        #endregion
        #region Drawers
        GridDrawer = GetTree().Root.GetNode<Control>("MainScene/DrawerGroup/GridDrawer");
        NoteDrawer = GetTree().Root.GetNode<Control>("MainScene/DrawerGroup/NoteDrawer");
        CursorDrawer = GetTree().Root.GetNode<Control>("MainScene/DrawerGroup/CursorDrawer");
        DrawerGroup = GetTree().Root.GetNode<DrawerGroup>("MainScene/DrawerGroup");
        GridDrawer.Position = DrawerPosition; GridDrawer.Size = DrawerSize; GridDrawer.Scale = DrawerScale;
        NoteDrawer.Position = DrawerPosition; NoteDrawer.Size = DrawerSize; NoteDrawer.Scale = DrawerScale;
        CursorDrawer.Position = DrawerPosition; CursorDrawer.Size = DrawerSize; CursorDrawer.Scale = DrawerScale;
        #endregion
        #region Players
        BGMPlayer = GetTree().Root.GetNode<AudioStreamPlayer>("MainScene/MediaPlayers/BGMPlayer");
        SEPlayerNormal = GetTree().Root.GetNode<AudioStreamPlayer>("MainScene/MediaPlayers/SEPlayerNormal");
        SEPlayerGold = GetTree().Root.GetNode<AudioStreamPlayer>("MainScene/MediaPlayers/SEPlayerGold");
        BGAPlayer = GetTree().Root.GetNode<VideoStreamPlayer>("MainScene/MediaPlayers/BGAPlayer");
        BGMPlayer.Finished += () =>
        {
            MediaManager.IsFinished = true;
        };
        #endregion
        #region Dialog
        Editor.Instance.LoadDialog = new FileDialog();
        Editor.Instance.LoadDialog.Access = FileDialog.AccessEnum.Filesystem;
        Editor.Instance.LoadDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        Editor.Instance.LoadDialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_load_dialog_file_selected)));
        Editor.Instance.LoadDialog.AddFilter("*.sdz", "SDZ Chart File");
        this.AddChild(Editor.Instance.LoadDialog);
        Editor.Instance.SaveDialog = new FileDialog();
        Editor.Instance.SaveDialog.Access = FileDialog.AccessEnum.Filesystem;
        Editor.Instance.SaveDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        Editor.Instance.SaveDialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_save_dialog_file_selected)));
        Editor.Instance.SaveDialog.AddFilter("*.sdz", "SDZ Chart File");
        this.AddChild(Editor.Instance.SaveDialog);
        #endregion
        EffectTrackLabel = GetTree().Root.GetNode("MainScene/TrackLabels").GetChild<Label>(3);
        SelectDisplayedEffectNoteType(NoteType.BPM);
    }
    void on_load_dialog_file_selected(string path)
    {
        if (!FileProcesser.LoadFromSDZ(path))
        {
            Editor.Instance.TipManager.AddTip("Error:加载此文件失败!", 0.5f, TipManager.TipIcon.Warning, TipManager.TipColor.Red);
        }
        else
        {
            SavePath = path;
            Editor.Saved = true;
            Editor.Instance.TipManager.AddTip("已成功加载!", 2.0f, TipManager.TipIcon.Information, TipManager.TipColor.Green);
            SyncTimeSystem.BuildBPMTimeLine();
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
    }
    void on_save_dialog_file_selected(string path)
    {
        SavePath = path;
        Editor.Saved = true;
        FileProcesser.ExportSDZ(path);
        TipManager.AddTip($"已保存到: {Editor.SavePath}!", 1.5f, TipManager.TipIcon.Information, TipManager.TipColor.Green);
    }

}
