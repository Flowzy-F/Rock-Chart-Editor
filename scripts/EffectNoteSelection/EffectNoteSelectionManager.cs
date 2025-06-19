using Godot;
using Godot.Collections;

public partial class EffectNoteSelectionManager : Control
{
    Button bpmButton;
    Button bkgButton;
    Button cameraButton;
    Vector2 markDefaultScale;
    Dictionary<Button, Sprite2D> buttonMarkPairs = new();
    public override void _Ready()
    {
        bpmButton = this.GetChild<Button>(2);
        bkgButton = this.GetChild<Button>(3);
        cameraButton = this.GetChild<Button>(4);
        Sprite2D bpmSelectedMark = bpmButton.GetChild<Sprite2D>(0);
        Sprite2D bkgSelectedMark = bkgButton.GetChild<Sprite2D>(0);
        Sprite2D cameraSelectedMark = cameraButton.GetChild<Sprite2D>(0);
        markDefaultScale = bpmSelectedMark.Scale;


        bpmButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(bpm_pressed)));
        bkgButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(bkg_pressed)));
        cameraButton.Connect(Button.SignalName.Pressed, new Callable(this, nameof(camera_pressed)));

        buttonMarkPairs.Add(bpmButton, bpmSelectedMark);
        buttonMarkPairs.Add(bkgButton, bkgSelectedMark);
        buttonMarkPairs.Add(cameraButton, cameraSelectedMark);
    }
    void buttonSelected(Button b)
    {
        foreach (var m in buttonMarkPairs.Values) m.Visible = false;
        if (!buttonMarkPairs.TryGetValue(b, out Sprite2D mark))
            return;
        mark.Scale = new Vector2(0, 0);
        mark.Visible = true;
        var tween = GetTree().CreateTween()
            .TweenProperty(mark, "scale", markDefaultScale, 0.2f)
            .SetTrans(Tween.TransitionType.Back);

    }
    void bpm_pressed()
    {
        buttonSelected(bpmButton);
        Editor.Instance.SelectDisplayedEffectNoteType(NoteInfo.NoteType.BPM);
    }
    void bkg_pressed()
    {
        buttonSelected(bkgButton);
        Editor.Instance.SelectDisplayedEffectNoteType(NoteInfo.NoteType.BKG);
    }
    void camera_pressed()
    {
        buttonSelected(cameraButton);
        Editor.Instance.SelectDisplayedEffectNoteType(NoteInfo.NoteType.Camera);
    }
}
