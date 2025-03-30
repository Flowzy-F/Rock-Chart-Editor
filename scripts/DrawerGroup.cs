using Godot;
using System;
using static Editor;

public partial class DrawerGroup : CanvasGroup
{
    /// <summary>
    /// <para><code>
    /// ----------1
    /// 
    /// 
    /// ----------
    ///  --You are here    ---
    ///                   offset
    /// ----------         ---
    /// 
    /// 
    /// ----------
    /// 
    /// 
    /// ----------0
    /// </code></para>
    /// </summary>
    public float HeightOffset = 0;
    public override void _Ready()
    {
        Bar = 0;
        Numerator = 0;
        RedrawAll();
    }

    public override void _Input(InputEvent @event)
    {
        Editor.Instance.MousePointedLocalCoord = ToLocalCoord(GetGlobalMousePosition());
        Editor.Instance.MousePointedBar = (int)Math.Floor(((float)Editor.Instance.MousePointedLocalCoord.Y + Numerator) / Denominator) + Bar;
        Editor.Instance.MousePointedTrack = Editor.Instance.MousePointedLocalCoord.X;
        Editor.Instance.MousePointedNumerator = (Editor.Instance.MousePointedLocalCoord.Y + Numerator) % Denominator;
    }
    public void RedrawAll()
    {
        Editor.Instance.GridDrawer.QueueRedraw();
        Editor.Instance.NoteDrawer.QueueRedraw();
        Editor.Instance.CursorDrawer.QueueRedraw();
    }
    public void UpdateHeightOffset()
    {
        HeightOffset = (TimeOffset - 1.0f / Denominator * Numerator)
                                         * (BeatHeight / Zoom);
    }
}
