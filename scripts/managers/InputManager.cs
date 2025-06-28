using Godot;
using System;
using System.IO;
using System.Linq;
using static Editor;
using static NoteInfo;
using static Utils;

public partial class InputManager : Node//So stupid of me to set it to Control just for a GetGlobalMousePosition().
{
    private Utils.Fraction temp_bpm_position;
    private InputBox input_box;
    public override void _Ready()
    {
        input_box = GD.Load<PackedScene>("res://scenes//input_box.tscn").Instantiate<InputBox>();
        input_box.Connect(AcceptDialog.SignalName.Confirmed, new Callable(this, nameof(place_bpm_note_when_confirmed)));
        AddChild(input_box);
        base._Ready();
    }
    public override void _Process(double delta)
    {
        //When a lineedit in FocusChangeNeedingLineEdits is handling input,don't trigger those inputs.
        if (Editor.Instance.FocusChangeNeedingLineEdits.All(n => !n.HasFocus()))
        {
            //Place Note
            if ((Input.IsActionJustReleased("PlaceNormal") || Input.IsActionJustPressed("PlaceGold") || Input.IsActionJustPressed("PlacePerformance"))
                && Editor.Instance.MousePointedBar >= 0)
            {
                if (Editor.Instance.MousePointedLocalCoord.Y >= TimeOffset
                    && ToLocalPosition(Editor.Instance.MousePointedLocalCoord).Y >= 0)//Only place when pointing on the grid.
                {
                    if (Editor.Instance.MousePointedTrack == TrackCount - 1 && !MediaManager.IsPlaying)
                    {
                        switch (DisplayedEffectNoteType)
                        {
                            case NoteType.BPM:
                                temp_bpm_position =
                                    new Utils.Fraction(Editor.Instance.MousePointedBar * Denominator + Editor.Instance.MousePointedNumerator, Denominator);
                                if (temp_bpm_position.Numerator == 0) return;//Don't place a bpm note at 0:0/0
                                if (NoteMap.ContainsKey(new NoteInfo.NoteHash(temp_bpm_position, NoteType.BPM)))
                                {
                                    input_box.Title = $"BPM变化Note {temp_bpm_position.GetWhole()}:{temp_bpm_position.GetTrueNumerator()}/{temp_bpm_position.Denominator}";
                                    input_box.SetHolderText("请输入新的BPM值。");
                                    input_box.SetTipText("修改BPM变化Note");
                                    input_box.SetLineEditText("");
                                }
                                else
                                {
                                    input_box.Title = "添加BPM变化Note";
                                    input_box.SetHolderText("请输入要变化到的BPM值。");
                                    input_box.SetTipText("添加BPM变化Note");
                                    input_box.SetLineEditText("");
                                }
                                input_box.PopupCentered(new Vector2I(350, 100));
                                break;
                            case NoteType.BKG:
                                Editor.Instance.PlaceBKGNote(new Utils.Fraction(Editor.Instance.MousePointedBar * Denominator + Editor.Instance.MousePointedNumerator, Denominator));
                                break;
                            case NoteType.Camera:
                                Utils.Fraction pointed_pos = new Utils.Fraction(Editor.Instance.MousePointedBar * Denominator + Editor.Instance.MousePointedNumerator, Denominator);
                                Editor.Instance.PlaceCameraNote(pointed_pos, Fraction.One(), EasingType.Linear,
                                    new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        HitColor t = HitColor.Normal;
                        if (Input.IsActionJustPressed("PlaceNormal"))
                            t = HitColor.Normal;
                        else if (Input.IsActionJustPressed("PlaceGold"))
                            t = HitColor.Gold;
                        else if (Input.IsActionJustPressed("PlacePerformance"))
                            t = HitColor.Performance;
                        if (Editor.Instance.PlaceHitNote(t, new Fraction(Editor.Instance.MousePointedBar * Denominator + Editor.Instance.MousePointedNumerator, Denominator),
                            Editor.Instance.MousePointedTrack, 1, 0, true))
                            NoteEditManager.UpdateNoteEdits();
                    }
                    Editor.Instance.NoteDrawer.QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("Play"))
            {
                NoteTriggeringManager.InitWhenStartPlaying();
                MediaManager.TogglePlay(SyncTimeSystem.ToTime(Bar + TimeOffset) + Offset);
            }
            if (Input.IsActionJustPressed("Delete"))
            {
                if (Editor.Instance.DeleteSelectedNotes())
                    Editor.Instance.NoteDrawer.QueueRedraw();
            }
            if (Input.IsActionJustPressed("Copy"))
            {
                if (SelectedNoteList.Count <= 0) return;
                NoteClipboard.Clear();
                foreach (NoteHash h in SelectedNoteList)
                {
                    if (NoteMap.TryGetValue(h, out NoteData d))
                    {
                        NoteClipboard.Add(d);
                    }
                    else
                        throw new Exception("Selected note(s) not found in NoteMap!");
                }
                NoteClipboard = NoteClipboard.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator).ToList();
            }
            if (Input.IsActionJustPressed("Cut"))
            {
                if (SelectedNoteList.Count > 0)
                {
                    NoteClipboard.Clear();
                    foreach (NoteHash h in SelectedNoteList)
                    {
                        if (NoteMap.TryGetValue(h, out NoteData d))
                        {
                            NoteClipboard.Add(d);
                        }
                        else
                            throw new Exception("Selected note(s) not found in NoteMap!");
                    }
                    Editor.Instance.DeleteSelectedNotes();
                    Editor.Instance.ClearSelectedNotes();
                    Editor.Instance.ClearCandidateNotes();
                    Editor.Instance.NoteDrawer.QueueRedraw();
                    NoteClipboard = NoteClipboard.OrderBy(n => (float)n.Position.Numerator / n.Position.Denominator).ToList();
                }
            }
            if (Input.IsActionJustPressed("Paste"))
            {
                if (Editor.Instance.Paste())
                    Editor.Instance.NoteDrawer.QueueRedraw();
            }
            if (Input.IsActionJustPressed("SelectAll"))
            {
                Editor.Instance.ClearSelectedNotes();
                Editor.Instance.ClearCandidateNotes();
                foreach (var h in NoteMap.Keys)
                {
                    if (h.NoteType == NoteType.Hit || h.NoteType == DisplayedEffectNoteType)
                        SelectedNoteList.Add(h);
                }
                NoteEditManager.UpdateNoteEdits();
                Editor.Instance.NoteDrawer.QueueRedraw();
            }
            if (Input.IsActionJustPressed("ui_left"))
            {
                if (Editor.Instance.MoveNote(SelectedNoteList, Fraction.Zero(), -1))
                {
                    Editor.Instance.NoteDrawer.QueueRedraw();
                    Editor.Instance.CursorDrawer.QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("ui_right"))
            {
                if (Editor.Instance.MoveNote(SelectedNoteList, Fraction.Zero(), 1))
                {
                    Editor.Instance.NoteDrawer.QueueRedraw();
                    Editor.Instance.CursorDrawer.QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("ui_up"))
            {
                if (Editor.Instance.MoveNote(SelectedNoteList, new Fraction(1, Denominator), 0))
                {
                    Editor.Instance.NoteDrawer.QueueRedraw();
                    Editor.Instance.CursorDrawer.QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("ui_down"))
            {
                if (Editor.Instance.MoveNote(SelectedNoteList, new Fraction(-1, Denominator), 0))
                {
                    Editor.Instance.NoteDrawer.QueueRedraw();
                    Editor.Instance.CursorDrawer.QueueRedraw();
                }
            }
            if (Input.IsActionJustPressed("Undo"))
            {
                if (CommandSystem.Undo())
                    Editor.Instance.NoteDrawer.QueueRedraw();
                if(SelectedNoteList.Count>0)
                    NoteEditManager.UpdateNoteEdits();
            }
            if (Input.IsActionJustPressed("Redo"))
            {
                if (CommandSystem.Redo())
                    Editor.Instance.NoteDrawer.QueueRedraw();
                if (SelectedNoteList.Count > 0)
                    NoteEditManager.UpdateNoteEdits();
            }
            if (Input.IsActionJustPressed("Save"))
            {
                if (File.Exists(SavePath))
                {
                    FileProcesser.ExportSDZ(SavePath);
                    Editor.Instance.TipManager.AddTip($"Saved at {SavePath}!", 1f, TipManager.TipIcon.Information, TipManager.TipColor.Green);
                }
                else
                {
                    Editor.Instance.SaveDialog.PopupCentered();
                }
            }
        }
    }
    private void place_bpm_note_when_confirmed()
    {
        if (float.TryParse(input_box.GetValue(), out float result) && result > 0)
        {

            Editor.Instance.PlaceBPMNoteOrModify(temp_bpm_position, result);
            Editor.Instance.NoteDrawer.QueueRedraw();
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton e)
        {
            if (e.ButtonIndex == MouseButton.WheelUp || e.ButtonIndex == MouseButton.WheelDown)
            {
                //Zoom
                if (Input.IsKeyPressed(Key.Ctrl))
                {
                    if (e.ButtonIndex == MouseButton.WheelUp)
                    {
                        Zoom = Math.Clamp(Zoom - e.Factor * 0.1f, ZoomMin, ZoomMax);
                    }
                    else if (e.ButtonIndex == MouseButton.WheelDown)
                    {
                        Zoom = Math.Clamp(Zoom + e.Factor * 0.1f, ZoomMin, ZoomMax);
                    }
                    Editor.Instance.DrawerGroup.UpdateHeightOffset();
                    Editor.Instance.DrawerGroup.RedrawAll();
                }
                //Move
                else if (!MediaManager.IsPlaying)
                {
                    if (e.ButtonIndex == MouseButton.WheelUp)
                    {
                        TimeOffset += e.Factor * (Input.IsKeyPressed(Key.Alt) ? 0.4f : 0.08f);
                        while (TimeOffset >= 1)
                        {
                            TimeOffset -= 1;
                            Bar++;
                        }
                        Editor.Instance.DrawerGroup.RedrawAll();
                    }
                    else if (e.ButtonIndex == MouseButton.WheelDown)
                    {
                        TimeOffset -= e.Factor * (Input.IsKeyPressed(Key.Alt) ? 0.4f : 0.08f);
                        while (TimeOffset < 0)
                        {
                            TimeOffset += 1;
                            Bar--;
                        }
                        if (Bar + TimeOffset <= BottomPosition)
                        {
                            TimeOffset = BottomPosition % 1.0f;
                            Bar = (int)Math.Ceiling(BottomPosition);
                        }
                    }
                    Editor.Instance.DrawerGroup.UpdateHeightOffset();
                    Editor.Instance.DrawerGroup.RedrawAll();
                }
                //Editor.Instance.DebugLabel.Text = $"Bar:{Bar}\nTimeOffset:{TimeOffset}";
            }
            if ((e.ButtonIndex == MouseButton.Left || e.ButtonIndex == MouseButton.Right) && e.Pressed)
            {
                foreach (var line_edit in Editor.Instance.FocusChangeNeedingLineEdits)
                {
                    if (!line_edit.GetGlobalRect().HasPoint(e.Position) && line_edit.HasFocus())
                    {
                        line_edit.ReleaseFocus();
                    }
                }
            }
            else if (e.ButtonIndex == MouseButton.Left)
            {
                if (!e.Pressed)//Only invoke when released
                {
                    if (IsMultipleSelecting)
                    {
                        IsMultipleSelecting = false;
                        Editor.Instance.SelectCandidateNotes();
                        NoteEditManager.UpdateNoteEdits();
                        Editor.Instance.NoteDrawer.QueueRedraw();
                        return;
                    }
                    if (Input.IsKeyPressed(Key.Alt))
                    {
                        if (CandidateNoteList.Count > 0)
                            if (NoteMap.TryGetValue(CandidateNoteList[0], out var d) && d.Type == NoteType.Hit)
                            {
                                ModificationRequest request = new ModificationRequest()
                                    .SetFor<HitNoteData, int>(data => data.RemoveCount, ((HitNoteData)d).RemoveCount + 1);
                                if (Editor.Instance.ModifyNoteData([CandidateNoteList[0]], request))
                                {
                                    Editor.Instance.NoteDrawer.QueueRedraw();
                                    NoteEditManager.UpdateNoteEdits();
                                }
                                return;
                            }
                    }
                    if (!Input.IsKeyPressed(Key.Ctrl) && !IsMultipleSelecting
                        && (new Rect2(DrawerPosition, DrawerDisplaySize).HasPoint(e.Position)
                        || CandidateNoteList.Count > 0))//Only when selecting a note outside the grid(Left click and candidate notes count>0)
                                                        //or clicking on nothing on the grid,
                                                        //the selecting notes get unselected.
                    {
                        Editor.Instance.ClearSelectedNotes();
                        NoteEditManager.UpdateNoteEdits();
                        Editor.Instance.NoteDrawer.QueueRedraw();
                    }
                    if (Input.IsKeyPressed(Key.Shift))
                    {
                        if (!IsMultipleSelecting)
                        {
                            IsMultipleSelecting = true;
                            Editor.Instance.MultipleSelectionCoord_Global = ToGlobalCoord(e.Position);
                            Editor.Instance.MultipleSelectionMouseOffset = e.Position - DrawerPosition
                                - ToLocalPosition(ToLocalCoord(e.Position));
                        }
                    }

                    if (!Editor.Instance.SelectCandidateNotes())//Select a line for pasting.
                    {
                        if (new Rect2(DrawerPosition, DrawerDisplaySize).HasPoint(e.Position))
                        //Select line only when click on the grid.
                        {
                            //GD.Print("Selected line.");
                            Editor.Instance.SelectedBarPosition =
                            new Fraction(Editor.Instance.MousePointedNumerator + Editor.Instance.MousePointedBar * Denominator, Denominator);
                            Editor.Instance.GridDrawer.QueueRedraw();
                        }
                    }
                    else
                    {

                        Editor.Instance.NoteDrawer.QueueRedraw();
                        NoteEditManager.UpdateNoteEdits();
                    }
                }
            }
            else if (e.ButtonIndex == MouseButton.Right)
            {
                if (!e.Pressed)//Only invoke when released
                {
                    if (IsMultipleSelecting)
                    {
                        IsMultipleSelecting = false;
                        foreach (var h in CandidateNoteList)
                        {
                            SelectedNoteList.Remove(h);
                        }
                        NoteEditManager.UpdateNoteEdits();
                        Editor.Instance.NoteDrawer.QueueRedraw();
                        return;
                    }
                    if (NoteMapBar.TryGetValue(Editor.Instance.MousePointedBar, out var hashes))
                    {
                        foreach (var h in hashes)
                        {
                            float pos_x = (h.Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                            float pos_y = DrawerDisplaySize.Y -
                               ((float)h.Position.Numerator / h.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom);
                            Vector2 pos = DrawerPosition + new Vector2(pos_x, pos_y);
                            if ((e.Position - pos).Length() <= NoteSize + 2)
                            {
                                if (Input.IsKeyPressed(Key.Ctrl))
                                {
                                    SelectedNoteList.Remove(h);
                                    Editor.Instance.NoteDrawer.QueueRedraw();
                                    break;
                                }
                                else if (Input.IsKeyPressed(Key.Alt))
                                {
                                    if (NoteMap.TryGetValue(h, out var d) && d.Type == NoteType.Hit)
                                    {
                                        ModificationRequest request = new ModificationRequest()
                                            .SetFor<HitNoteData, int>(data => data.RemoveCount, ((HitNoteData)d).RemoveCount - 1);
                                        if (Editor.Instance.ModifyNoteData([h], request))
                                        {
                                            Editor.Instance.NoteDrawer.QueueRedraw();
                                            NoteEditManager.UpdateNoteEdits();

                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    Editor.Instance.DeleteNote(h, 1);
                                    Editor.Instance.NoteDrawer.QueueRedraw();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (@event is InputEventMouse e2)
        {
            //UpdateCandidate
            Editor.Instance.ClearCandidateNotes();
            if (IsMultipleSelecting)
            {
                Editor.Instance.MultipleSelectionStartPos = ToGlobalPosition(Editor.Instance.MultipleSelectionCoord_Global)
                    + Editor.Instance.MultipleSelectionMouseOffset;
                float start_x = Math.Min(Editor.Instance.MultipleSelectionStartPos.X, e2.Position.X - DrawerPosition.X);
                float start_y = Math.Min(Editor.Instance.MultipleSelectionStartPos.Y, e2.Position.Y - DrawerPosition.Y);
                float width = Math.Abs(Editor.Instance.MultipleSelectionStartPos.X - (e2.Position.X - DrawerPosition.X));
                float height = Math.Abs(Editor.Instance.MultipleSelectionStartPos.Y - (e2.Position.Y - DrawerPosition.Y));
                Editor.Instance.MultipleSelectionRegionRect = new Rect2(start_x, start_y, width, height);
                //Detect Notes in the region
                int start_bar = Math.Min(Editor.Instance.MousePointedBar, (int)Math.Floor(
                    (float)Editor.Instance.MultipleSelectionCoord_Global.Y / Denominator));
                int end_bar = Math.Max(Editor.Instance.MousePointedBar, (int)Math.Floor(
                    (float)Editor.Instance.MultipleSelectionCoord_Global.Y / Denominator));
                for (int i = start_bar; i <= end_bar; i++)
                {
                    if (NoteMapBar.TryGetValue(i, out var hashes))
                    {
                        foreach (var h in hashes)
                        {
                            float pos_x = 0;
                            float pos_y = DrawerDisplaySize.Y -
                               ((float)h.Position.Numerator / h.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom); ;
                            if (h.NoteType == NoteType.Hit)
                                pos_x = (h.Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                            else
                                //Effect Notes,these notes are drawn at the last track,but there actual track is not TrackCount-1.
                                pos_x = (TrackCount - 1 + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);

                            if ((h.NoteType == NoteType.Hit || DisplayedEffectNoteType == h.NoteType) &&
                                Editor.Instance.MultipleSelectionRegionRect.HasPoint(new Vector2(pos_x, pos_y)))
                            {
                                Editor.Instance.AddCandidateNote(h);
                            }
                        }
                    }
                }
            }
            if (NoteMapBar.TryGetValue(Editor.Instance.MousePointedBar, out var list))
            {
                foreach (var h in list)
                {
                    float pos_x = 0;
                    float pos_y = DrawerDisplaySize.Y -
                       ((float)h.Position.Numerator / h.Position.Denominator - (Bar + TimeOffset)) * (BeatHeight / Zoom);

                    if (h.NoteType == NoteType.Hit)
                        pos_x = (h.Track + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);
                    else
                        //Effect Notes,these notes are drawn at the last track,but there actual track is not TrackCount-1.
                        pos_x = (TrackCount - 1 + NoteXOffset) * (DrawerDisplaySize.X / TrackCount);

                    Vector2 pos = DrawerPosition + new Vector2(pos_x, pos_y);
                    if (
                        (h.NoteType == NoteType.Hit || DisplayedEffectNoteType == h.NoteType) &&
                        (e2.Position - pos).Length() <= NoteSize + 2
                        )
                    {
                        Editor.Instance.AddCandidateNote(h);
                        break;
                    }
                }
            }
            Editor.Instance.CursorDrawer.QueueRedraw();
        }
        base._Input(@event);
    }
}
