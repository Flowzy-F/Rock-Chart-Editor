using Godot;
using System;
using System.IO;

public partial class Menubar : MenuBar
{
    PopupMenu file_menu;
    PopupMenu edit_menu;
    PopupMenu chart_menu;
    PopupMenu about_menu;
    ConfirmationDialog load_confirmation;
    ChartInfoDialog chart_info_dialog;
    SettingsDialog settings_dialog;
    AcceptDialog about_dialog;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        file_menu = GetTree().Root.GetNode<PopupMenu>("MainScene/MenuBar/文件");
        file_menu.Connect(PopupMenu.SignalName.IdPressed, new Callable(this, nameof(on_file_menu_idpressed)));
        chart_menu = GetTree().Root.GetNode<PopupMenu>("MainScene/MenuBar/谱面");
        chart_menu.Connect(PopupMenu.SignalName.IdPressed, new Callable(this, nameof(on_chart_menu_idpressed)));
        edit_menu = GetTree().Root.GetNode<PopupMenu>("MainScene/MenuBar/编辑");
        edit_menu.Connect(PopupMenu.SignalName.IdPressed, new Callable(this, nameof(on_edit_menu_idpressed)));
        about_menu = GetTree().Root.GetNode<PopupMenu>("MainScene/MenuBar/关于");
        about_menu.Connect(PopupMenu.SignalName.IdPressed, new Callable(this, nameof(on_about_menu_idpressed)));

        load_confirmation = new ConfirmationDialog();
        load_confirmation.OkButtonText = "保存";
        load_confirmation.CancelButtonText = "算了";
        load_confirmation.Title = "从文件中加载谱面";
        load_confirmation.DialogText = "当前谱面未保存，要进行保存吗?";
        load_confirmation.Connect(ConfirmationDialog.SignalName.Confirmed, new Callable(this, nameof(on_load_confirmation_confirmed)));
        load_confirmation.Connect(ConfirmationDialog.SignalName.Canceled, new Callable(this, nameof(on_load_confirmation_canceled)));
        this.AddChild(load_confirmation);

        chart_info_dialog = GD.Load<PackedScene>("res://scenes//chart_info_dialog.tscn").Instantiate<ChartInfoDialog>();
        this.AddChild(chart_info_dialog);
        chart_info_dialog.Connect(AcceptDialog.SignalName.Confirmed, new Callable(this, nameof(chart_info_dialog_confirmed)));
        settings_dialog = GD.Load<PackedScene>("res://scenes//settings_dialog.tscn").Instantiate<SettingsDialog>();
        this.AddChild(settings_dialog);
        about_dialog = GD.Load<PackedScene>("res://scenes//about_dialog.tscn").Instantiate<AcceptDialog>();
        this.AddChild(about_dialog);

    }
    void on_file_menu_idpressed(int id)
    {
        switch (id)
        {
            case 0:
                if (!Editor.Saved)
                {
                    load_confirmation.PopupCentered();
                }

                else
                    Editor.Instance.LoadDialog.PopupCentered();
                break;
            case 1:
                Editor.Instance.SaveDialog.PopupCentered();
                break;
            default:
                break;
        }
    }
    async void on_load_confirmation_confirmed()
    {
        if (!File.Exists(Editor.SavePath))
        {
            Editor.Instance.SaveDialog.PopupCentered();
            await ToSignal(Editor.Instance.SaveDialog, FileDialog.SignalName.VisibilityChanged);
            //UNDONE: use FileDialog.SignalName.VisibilityChanged here to wait until the dialog closes or selected file.
            await ToSignal(GetTree().CreateTimer(0.01), Timer.SignalName.Timeout);//To avoid Exclusive dialog confliction.
        }
        else
        {
            FileProcesser.ExportSDZ(Editor.SavePath);
            Editor.Instance.TipManager.AddTip($"已保存在: {Editor.SavePath}!", 1.5f, TipManager.TipIcon.Information, TipManager.TipColor.Green);
        }
        Editor.Instance.LoadDialog.PopupCentered();
    }
    async void on_load_confirmation_canceled()
    {
        await ToSignal(GetTree().CreateTimer(0.01), Timer.SignalName.Timeout);//To avoid Exclusive dialog confliction.
        Editor.Instance.LoadDialog.PopupCentered();
    }
    void on_chart_menu_idpressed(int id)
    {
        switch (id)
        {
            case 0:
                if (MediaManager.IsPlaying)
                    MediaManager.Pause();
                chart_info_dialog.UpdateDisplay();
                chart_info_dialog.PopupCentered();
                break;
            default:
                break;
        }
    }
    void on_edit_menu_idpressed(int id)
    {
        switch (id)
        {
            case 0:
                if (MediaManager.IsPlaying)
                    MediaManager.Pause();
                settings_dialog.PopupCentered();
                break;
            default:
                break;
        }
    }
    void on_about_menu_idpressed(int id)
    {
        switch (id)
        {
            case 0:
                System.Diagnostics.Process.Start("explorer.exe", "https://github.com/Flowzy-F/Rock-Chart-Editor");
                break;
            case 1:

                about_dialog.PopupCentered();
                break;
            default:
                break;
        }
    }
    void chart_info_dialog_confirmed()
    {
        chart_info_dialog.GetValues(out string title,
            out string artist,
            out string mapper,
            out string mass,
            out Difficulty difficulty,
            out string bpm,
            out string offset,
            out string bg_offset,
            out string bgm_path,
            out string bga_path);
        Editor.Title = title;
        Editor.Artist = artist;
        Editor.Mapper = mapper;
        Editor.Diff = difficulty;
        if (int.TryParse(mass, out int mass_r))
        {
            Editor.Mass = mass_r;
        }
        else
        {
            Editor.Instance.TipManager.AddTip("定数未修改。请输入一个有效的整数值。", 3.0f,
                TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }
        if (float.TryParse(bpm, out float bpm_r) && bpm_r > 0)
        {
            Editor.BPM = bpm_r;
            SyncTimeSystem.BuildBPMTimeLine();
        }
        else
        {
            Editor.Instance.TipManager.AddTip("基础BPM未修改。请输入一个有效的浮点数值。", 3.0f,
                TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }

        if (float.TryParse(offset, out float offset_r) && offset_r >= 0)
        {
            Editor.Offset = offset_r;
        }
        else if (offset.Trim() != "")
        {
            Editor.Instance.TipManager.AddTip("谱面延迟未修改。请输入一个有效的浮点数值。", 3.0f,
                TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }

        if (float.TryParse(bg_offset, out float bg_offset_r))
        {
            Editor.BGOffset = bg_offset_r;
        }
        else if (bg_offset.Trim() != "")
        {
            Editor.Instance.TipManager.AddTip("BGA延迟未修改。请输入一个有效的浮点数值。", 3.0f,
                TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }

        if (File.Exists(bgm_path))
        {
            Editor.BGMPath = bgm_path;
        }
        else
        {
            //Editor.Instance.TipManager.AddTip("BGM File not found.", 2.0f,
            //    TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }

        if (File.Exists(bga_path))
        {
            Editor.BGAPath = bga_path;
        }
        else
        {
            //Editor.Instance.TipManager.AddTip("BGA File not found.", 2.0f,
            //    TipManager.TipIcon.Information, TipManager.TipColor.Yellow);
        }
    }
    public string GetSavePath()
    {
        return Editor.Instance.SaveDialog.CurrentFile;
    }
}
