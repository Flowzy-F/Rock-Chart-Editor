using Godot;
using NAudio.Wave;
using System.IO;

public partial class SettingsDialog : AcceptDialog
{
    OptionButton normal_hit_sound_option;
    OptionButton gold_hit_sound_option;
    Button normal_load_button;
    FileDialog normal_sound_file_dialog;
    Button gold_load_button;
    FileDialog gold_sound_file_dialog;
    CheckButton same_sound_checkbutton;
    public override void _Ready()
    {
        normal_hit_sound_option = this.GetChild(0).GetChild(0).GetChild(0).GetChild<OptionButton>(1);
        normal_hit_sound_option.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(on_normal_hit_sound_option_selected)));

        gold_hit_sound_option = this.GetChild(0).GetChild(0).GetChild(1).GetChild<OptionButton>(1);
        gold_hit_sound_option.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(on_gold_hit_sound_option_selected)));
        same_sound_checkbutton = this.GetChild(0).GetChild(0).GetChild(1).GetChild<CheckButton>(2);
        same_sound_checkbutton.Connect(CheckButton.SignalName.Pressed, new Callable(this, nameof(on_same_sound_button_pressed)));
        #region FileDialogSound
        normal_load_button = this.GetChild(0).GetChild(0).GetChild(0).GetChild<Button>(2);
        normal_load_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(on_normal_load_button_pressed)));
        normal_sound_file_dialog = new FileDialog();
        normal_sound_file_dialog.Access = FileDialog.AccessEnum.Filesystem;
        normal_sound_file_dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        normal_sound_file_dialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_normal_load_file_selected)));
        normal_sound_file_dialog.AddFilter("*.mp3,*.wav,*.ogg", "Audio File");
        this.AddChild(normal_sound_file_dialog);
        gold_load_button = this.GetChild(0).GetChild(0).GetChild(1).GetChild<Button>(4);
        gold_load_button.Connect(Button.SignalName.Pressed, new Callable(this, nameof(on_gold_load_button_pressed)));
        gold_sound_file_dialog = new FileDialog();
        gold_sound_file_dialog.Access = FileDialog.AccessEnum.Filesystem;
        gold_sound_file_dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        gold_sound_file_dialog.Connect(FileDialog.SignalName.FileSelected, new Callable(this, nameof(on_gold_load_file_selected)));
        gold_sound_file_dialog.AddFilter("*.mp3,*.wav,*.ogg", "Audio File");
        this.AddChild(gold_sound_file_dialog);
        #endregion
    }

    void on_normal_hit_sound_option_selected(int selected)
    {
        switch (selected)
        {
            case 0:
                Editor.Instance.SEPlayerNormal.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//normal//hit_normal.wav");
                break;
            case 1:
                Editor.Instance.SEPlayerNormal.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//normal//hit_kick.wav");
                break;
            case 2:
                Editor.Instance.SEPlayerNormal.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//normal//hit_snare.wav");
                break;
            default:
                break;
        }
        if (same_sound_checkbutton.ButtonPressed)
            gold_hit_sound_option.Select(gold_hit_sound_option.Selected);
    }
    void on_gold_hit_sound_option_selected(int selected)
    {
        if (same_sound_checkbutton.ButtonPressed) return;
        switch (selected)
        {
            case 0:
                Editor.Instance.SEPlayerGold.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//gold//hit_coin_gold.wav");
                break;
            case 1:
                Editor.Instance.SEPlayerGold.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//gold//hit_kick_gold.wav");
                break;
            case 2:
                Editor.Instance.SEPlayerGold.Stream = GD.Load<AudioStream>("res://sounds//hit_sound//gold//hit_snare_gold.wav");
                break;
            default:
                break;
        }
    }
    void on_same_sound_button_pressed()
    {
        if (same_sound_checkbutton.ButtonPressed)
        {
            gold_hit_sound_option.Disabled = true;
            gold_load_button.Disabled = true;
            Editor.Instance.SEPlayerGold.Stream = Editor.Instance.SEPlayerNormal.Stream;
        }
        else
        {
            gold_hit_sound_option.Disabled = false;
            gold_load_button.Disabled = false;
            gold_hit_sound_option.Select(2);
        }
    }
    void on_normal_load_button_pressed()
    {
        normal_sound_file_dialog.PopupCentered();
    }
    void on_gold_load_button_pressed()
    {
        gold_sound_file_dialog.PopupCentered();
    }
    void on_normal_load_file_selected(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File doesn't Exist: " + path);

        string extension = Path.GetExtension(path).ToLower();

        switch (extension)
        {
            case ".mp3":
                Editor.Instance.SEPlayerNormal.Stream = new AudioStreamMP3 { Data = File.ReadAllBytes(path) };
                break;
            case ".wav":
                using (var reader = new AudioFileReader(path))
                {
                    var format = new WaveFormat(44100, 16, 2); //Fix the format to fit the AudioStream.
                    var resampler = new MediaFoundationResampler(reader, format);
                    byte[] buffer = new byte[reader.Length];
                    int bytesRead = resampler.Read(buffer, 0, buffer.Length);

                    Editor.Instance.SEPlayerNormal.Stream = new AudioStreamWav
                    {
                        Data = buffer,
                        Format = AudioStreamWav.FormatEnum.Format16Bits,
                        MixRate = (int)format.SampleRate,
                        Stereo = format.Channels == 2
                    };
                }
                break;
            case ".ogg":
                Editor.Instance.SEPlayerNormal.Stream = AudioStreamOggVorbis.LoadFromFile(path);
                break;
            default:
                break;
        }
        normal_hit_sound_option.Selected = normal_hit_sound_option.ItemCount - 1;
        Editor.Instance.TipManager.AddTip($"Successfully Loaded!", 1.5f, TipManager.TipIcon.Information, TipManager.TipColor.Green);
    }
    void on_gold_load_file_selected(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File doesn't Exist: " + path);

        string extension = Path.GetExtension(path).ToLower();
        switch (extension)
        {
            case ".mp3":
                Editor.Instance.SEPlayerGold.Stream = new AudioStreamMP3 { Data = File.ReadAllBytes(path) };
                break;
            case ".wav":
                using (var reader = new AudioFileReader(path))
                {
                    var format = new WaveFormat(44100, 16, 2); //Fix the format to fit the AudioStream.
                    var resampler = new MediaFoundationResampler(reader, format);
                    byte[] buffer = new byte[reader.Length];
                    int bytesRead = resampler.Read(buffer, 0, buffer.Length);

                    Editor.Instance.SEPlayerGold.Stream = new AudioStreamWav
                    {
                        Data = buffer,
                        Format = AudioStreamWav.FormatEnum.Format16Bits,
                        MixRate = (int)format.SampleRate,
                        Stereo = format.Channels == 2
                    };
                }
                break;
            case ".ogg":
                Editor.Instance.SEPlayerGold.Stream = AudioStreamOggVorbis.LoadFromFile(path);
                break;
            default:
                break;
        }
        gold_hit_sound_option.Selected = gold_hit_sound_option.ItemCount - 1;
        Editor.Instance.TipManager.AddTip($"Successfully Loaded!", 1.5f, TipManager.TipIcon.Information, TipManager.TipColor.Green);

    }
}
