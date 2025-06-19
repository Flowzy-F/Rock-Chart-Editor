using Godot;

public partial class EffectNoteButton : Button
{
    Sprite2D selectedMark;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        selectedMark = GetChild<Sprite2D>(0);
        Connect(Button.SignalName.MouseEntered, new Callable(this, nameof(on_mouse_enter)));
        Connect(Button.SignalName.ButtonDown, new Callable(this, nameof(on_mouse_down)));
        Connect(Button.SignalName.MouseExited, new Callable(this, nameof(reset_appearance)));
        Connect(Button.SignalName.ButtonUp, new Callable(this, nameof(on_mouse_up)));
    }

    void on_mouse_enter()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            on_mouse_down();
        else
            this.Scale = new Vector2(0.9f, 0.9f);
    }
    void on_mouse_down()
    {
        this.Scale = new Vector2(0.8f, 0.8f);
    }
    void on_mouse_up()
    {
        if (new Rect2(GlobalPosition,Size).HasPoint(GetGlobalMousePosition()))
            on_mouse_enter();
        else
            reset_appearance();
    }
    void reset_appearance()
    {
        this.Scale = new Vector2(1.0f, 1.0f);
    }

}
