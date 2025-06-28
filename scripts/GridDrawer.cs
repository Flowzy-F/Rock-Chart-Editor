using Godot;
using System;
using static Editor;
public partial class GridDrawer : Control
{
    Editor editor;
    Label label1;
    Label label2;
    Label label3;
    Label label4;
    public override void _Ready()
    {
        editor = GetNode<Editor>("/root/Editor");
        label1 =GetTree().Root.GetNode("MainScene/TrackLabels").GetChild<Label>(0) ;
        label2 =GetTree().Root.GetNode("MainScene/TrackLabels").GetChild<Label>(1) ;
        label3 =GetTree().Root.GetNode("MainScene/TrackLabels").GetChild<Label>(2) ;
        label4 =GetTree().Root.GetNode("MainScene/TrackLabels").GetChild<Label>(3) ;
        label1.Position = new Vector2(this.Position.X + this.Size.X / 4 * 0+15, label1.Position.Y);
        label2.Position = new Vector2(this.Position.X + this.Size.X / 4 * 1+15, label2.Position.Y);
        label3.Position = new Vector2(this.Position.X + this.Size.X / 4 * 2+15, label3.Position.Y);
        label4.Position = new Vector2(this.Position.X + this.Size.X / 4 * 3+15, label4.Position.Y);
        base._Ready();
    }
    public override void _Draw()
    {
        draw_grid();
        base._Draw();
    }
    void draw_grid()
    {
        //Virtical
        float unit_width = DrawerDisplaySize.X / TrackCount;
        for (int i = 0; i < TrackCount + 1; i++)
        {
            DrawLine(new Vector2(i * unit_width, 0),
                new Vector2(0 + i * unit_width, DrawerDisplaySize.Y),
                NormalLineColor);
        }
        //Horizontal
        int visible_line_count = (int)Math.Ceiling(DrawerDisplaySize.Y / (BeatHeight / Denominator / Zoom))
            + (Editor.Instance.DrawerGroup.HeightOffset / (BeatHeight / Denominator / Zoom) >= 0.5f ? 1 : 0);
        float line_interval = BeatHeight / Denominator / Zoom;
        for (int i = TimeOffset == 0 ? 0 : 1; i < visible_line_count; i++)//Don't draw the line below the bottom line
        {

            float pos_y = DrawerDisplaySize.Y - (i * line_interval - Editor.Instance.DrawerGroup.HeightOffset);
            Color line_color = NormalLineColor;
            if ((Numerator + i) % Denominator == 0)
            {
                line_color = BarLineColor;
                DrawString(Editor.Instance.DefaultFont, new Vector2(DrawerDisplaySize.X + 10, pos_y), ((Numerator + i) / Denominator + Bar).ToString());
            }
            else
                line_color = NormalLineColor;
            DrawLine(new Vector2(0, pos_y), new Vector2(DrawerDisplaySize.X, pos_y), line_color);
        }
        //SelectedLine
        float selected_line_position_local = ((float)Editor.Instance.SelectedBarPosition.Numerator / Editor.Instance.SelectedBarPosition.Denominator - (Bar + TimeOffset))
            * BeatHeight / Zoom;
        if (selected_line_position_local >= 0 && selected_line_position_local <= DrawerDisplaySize.Y)
        {
            float pos_y = DrawerDisplaySize.Y - selected_line_position_local;
            DrawLine(new Vector2(0, pos_y), new Vector2(DrawerDisplaySize.X, pos_y), SelectedLineColor);
        }
        //Bottom Judgement Line
        DrawLine(new Vector2(0, DrawerDisplaySize.Y), new Vector2(DrawerDisplaySize.X + 30, DrawerDisplaySize.Y), new Color(255, 255, 255));
    }
}
