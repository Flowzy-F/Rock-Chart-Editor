using Godot;
using System.IO;
using static Editor;
using NAudio.Wave;
public partial class MediaManager
{
    public static bool IsPlaying { get; set; } = false;
    public static bool IsFinished { get; set; } = false;
    public static void LoadBGMFromPath(string path)
    {
        Editor.Instance.BGMPlayer.StreamPaused = true;
        if (!File.Exists(path))
            throw new FileNotFoundException("File doesn't Exist: " + path);

        string extension = Path.GetExtension(path).ToLower();

        switch (extension)
        {
            case ".mp3":
                Editor.Instance.BGMPlayer.Stream = new AudioStreamMP3 { Data = File.ReadAllBytes(path) };
                break;
            case ".wav":
                using (var reader = new AudioFileReader(path))
                {
                    var format = new WaveFormat(44100, 16, 2); //Fix the format to fit the AudioStream.
                    var resampler = new MediaFoundationResampler(reader, format);
                    byte[] buffer = new byte[reader.Length];
                    int bytesRead = resampler.Read(buffer, 0, buffer.Length);

                    Editor.Instance.BGMPlayer.Stream = new AudioStreamWav
                    {
                        Data = buffer,
                        Format = AudioStreamWav.FormatEnum.Format16Bits,
                        MixRate = (int)format.SampleRate,
                        Stereo = format.Channels == 2
                    };
                }
                break;
            default:
                break;
        }
    }
    public static void LoadBGAFromPath(string path)
    {

    }
    public static void Pause()
    {
        if (!IsPlaying) return;
        Editor.Instance.BGMPlayer.StreamPaused = true;
        IsPlaying = false;
    }
    public static void TogglePlay(float start_pos)
    {
        if (!IsPlaying)
        {
            IsFinished = false;
            if (Editor.Instance.BGMPlayer.Stream == null || start_pos >= Editor.Instance.BGMPlayer.Stream.GetLength())
            {
                IsFinished = true;
                IsPlaying = true;
                return;
            }
            Editor.Instance.BGMPlayer.StreamPaused = false;
            Editor.Instance.BGMPlayer.Play(start_pos);
        }
        else
        {
            Editor.Instance.BGMPlayer.StreamPaused = true;
        }
        IsPlaying = !IsPlaying;
    }
    public static double GetBGMLength()
    {
        return Editor.Instance.BGMPlayer.Stream.GetLength();
    }
    public static float GetPlaybackPosition()
    {
        return (float)(Editor.Instance.BGMPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix()
            - AudioServer.GetOutputLatency());
    }
}
