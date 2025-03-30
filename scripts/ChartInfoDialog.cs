using Godot;
using System;
using System.IO;
using static Editor;

public partial class ChartInfoDialog : AcceptDialog
{
    LineEdit title_line_edit;
    LineEdit artist_line_edit;
    LineEdit mapper_line_edit;
    LineEdit level_line_edit;
    LineEdit bpm_line_edit;
    LineEdit offset_line_edit;
    LineEdit bg_offset_line_edit;
    Button bgm_path_button;
    Label bgm_path_label;
    Button bga_path_button;
    Label bga_path_label;
    FileDialog bgm_dialog;
    FileDialog bga_dialog;
    public override void _Ready()
    {
        title_line_edit = this.GetChild(0).GetChild(0).GetChild<LineEdit>(1);
        artist_line_edit = this.GetChild(0).GetChild(1).GetChild<LineEdit>(1);
        mapper_line_edit = this.GetChild(0).GetChild(2).GetChild<LineEdit>(1);
        level_line_edit = this.GetChild(0).GetChild(3).GetChild<LineEdit>(1);
        bpm_line_edit = this.GetChild(0).GetChild(4).GetChild<LineEdit>(1);
        offset_line_edit = this.GetChild(0).GetChild(5).GetChild<LineEdit>(1);
        bg_offset_line_edit = this.GetChild(0).GetChild(6).GetChild<LineEdit>(1);
        bgm_path_button = this.GetChild(0).GetChild(7).GetChild<Button>(0);
        bgm_path_label = this.GetChild(0).GetChild(7).GetChild<Label>(1);
        bga_path_button = this.GetChild(0).GetChild(8).GetChild<Button>(0);
        bga_path_label = this.GetChild(0).GetChild(8).GetChild<Label>(1);

        bgm_path_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(call_bgm_dialog)));
        bga_path_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(call_bga_dialog)));

        bgm_dialog = new FileDialog();
        bgm_dialog.Access = FileDialog.AccessEnum.Filesystem;
        bgm_dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        bgm_dialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_bgm_selected)));
        bgm_dialog.AddFilter("*.mp3,*.wav", "Audio File");
        this.AddChild(bgm_dialog);
        bga_dialog = new FileDialog();
        bga_dialog.Access = FileDialog.AccessEnum.Filesystem;
        bga_dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        bga_dialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_bga_selected)));
        bga_dialog.AddFilter("*.mp4", "Video File");
        this.AddChild(bga_dialog);
    }
    public void UpdateDisplay()
    {
        title_line_edit.Text = Editor.Title;//Don't delete this "Editor.".
        artist_line_edit.Text = Artist;
        mapper_line_edit.Text=Mapper;
        level_line_edit.Text = Level.ToString();
        bpm_line_edit.Text = BPM.ToString();
        offset_line_edit.Text = Offset==0?"":Offset.ToString();
        bg_offset_line_edit.Text = BGOffset == 0 ? "":BGOffset.ToString();
        bgm_path_label.Text = BGMPath;
        //bga_path_label.Text=BGAPath;
    }
    public void GetValues(out string title,out string artist,out string mapper,out string level,out string bpm,
        out string offset,out string bg_offset,out string bgm_path,out string bga_path)
    {
        title = title_line_edit.Text;
        artist= artist_line_edit.Text;
        mapper= mapper_line_edit.Text;
        level = level_line_edit.Text;
        bpm=bpm_line_edit.Text;
        offset= offset_line_edit.Text;
        bg_offset= bg_offset_line_edit.Text;
        bgm_path= bgm_path_label.Text;
        bga_path= bga_path_label.Text;
    }
    void call_bgm_dialog()
    {
        bgm_dialog.PopupCentered();
    }
    void call_bga_dialog()
    {
        bga_dialog.PopupCentered();
    }
    void on_bgm_selected(string path)
    {
        MediaManager.LoadBGMFromPath(path);
        bgm_path_label.Text = path;
    }
    void on_bga_selected(string path)
    {
        MediaManager.LoadBGAFromPath(path);
        //bga_path_label.Text = path;
    }
}
