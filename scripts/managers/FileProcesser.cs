using Godot;
using System.IO;
using System.Linq;
using static Editor;
using static Utils;
using static NoteInfo;
using System.Collections.Generic;

public partial class FileProcesser
{
    public static void ExportSDZ(string path)
    {
        StreamWriter sw = new StreamWriter(path, false);
        sw.WriteLine("[Meta]");
        sw.WriteLine($"title={Title}");
        sw.WriteLine($"author={Artist}");
        sw.WriteLine($"mapper={Mapper}");
        sw.Write("difficulty=");
        switch (Diff)
        {
            case Difficulty.None:
                sw.WriteLine("None");
                break;
            case Difficulty.Fun:
                sw.WriteLine("Fun");
                break;
            case Difficulty.Easy:
                sw.WriteLine("Easy");
                break;
            case Difficulty.Normal:
                sw.WriteLine("Normal");
                break;
            case Difficulty.Hard:
                sw.WriteLine("Hard");
                break;
            case Difficulty.Powerful:
                sw.WriteLine("Powerful");
                break;
            case Difficulty.SoPowerful:
                sw.WriteLine("So:Powerful");
                break;
        }
        sw.WriteLine($"mass={Mass}");
        sw.WriteLine($"bpm={BPM}");
        sw.WriteLine($"offset={Offset}");
        sw.WriteLine($"bg_offset={BGOffset}");

        sw.WriteLine("[Data]");
        Dictionary<NoteHash, NoteData> ordered_datas = NoteMap.OrderBy(n => (float)n.Value.Position.Numerator / n.Value.Position.Denominator).ToDictionary();
        Fraction current_segment_start_pos = new Fraction(0, 1);
        foreach (NoteData d in ordered_datas.Values)
        {
            Fraction segment_local_pos = d.Position - current_segment_start_pos;
            if (d.Type == NoteType.Hit)
            {
                char type_char = ' ';
                if (((HitNoteData)d).Color == HitColor.Normal) type_char = 'D';
                else if (((HitNoteData)d).Color == HitColor.Gold) type_char = 'X';
                else if (((HitNoteData)d).Color == HitColor.Performance) type_char = 'S';
                sw.WriteLine($"{type_char},{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator},{((HitNoteData)d).Track + 1},{((HitNoteData)d).Count},{((HitNoteData)d).RemoveCount}," +
                    $"{((HitNoteData)d).Scale},{((HitNoteData)d).YOffset}");

            }
            else if (d.Type == NoteType.BPM)
            {
                sw.WriteLine($"B,{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator},{((BPMNoteData)d).BPMValue}");
                current_segment_start_pos = d.Position;
            }
            else if (d.Type == NoteType.BKG)
            {
                sw.WriteLine($"H,{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator}");
            }
            else if (d.Type == NoteType.Camera)
            {
                sw.WriteLine($"C,{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator}," +
                    $"{(int)((CameraNoteData)d).Easing+1}," +
                    $"{((CameraNoteData)d).Duration.GetWhole()}," +
                    $"{((CameraNoteData)d).Duration.GetTrueNumerator()}," +
                    $"{((CameraNoteData)d).Duration.Denominator}," +
                    $"{((CameraNoteData)d).RotationMovementZ}," +
                    $"{((CameraNoteData)d).RotationMovementY}," +
                    $"{((CameraNoteData)d).RotationMovementX}," +
                    $"{((CameraNoteData)d).PositionMovementZ}," +
                    $"{((CameraNoteData)d).PositionMovementY}," +
                    $"{((CameraNoteData)d).PositionMovementX}"
                    );
            }
        }
        sw.Flush();
        sw.Close();
    }
    public static bool LoadFromSDZ(string path)
    {
        if (!File.Exists(path)) return false;
        Editor.Instance.ClearNotes();
        CommandSystem.ClearStack();
        StreamReader sr = new StreamReader(path);
        string line = "";
        bool read_a_valid_line()
        {
            line = sr.ReadLine();
            if (line == null)
            {
                return false;
            }
            while ((line.Length >= 2 && line.Substring(0, 2) == "//") || line == "")
            {
                line = sr.ReadLine().Trim();
            }
            return true;
        }
        read_a_valid_line();
        if (line == "[Meta]")
        {
            //Meta
            while (line != "[Data]")
            {
                read_a_valid_line();
                string info = line.Split('=')[0];
                switch (info.Trim())
                {
                    case "title":
                        Title = line.Substring(info.Length + 1).TrimStart();
                        break;
                    case "author":
                        Artist = line.Substring(info.Length + 1).TrimStart();
                        break;
                    case "mapper":
                        Mapper = line.Substring(info.Length + 1).TrimStart();
                        break;
                    case "mass":
                    case "level":
                        if (int.TryParse(line.Substring(info.Length + 1).Trim(), out int result1) && result1 >= 0)
                            Mass = result1;
                        else
                        {
                            Mass = 0;
                        }
                        break;
                    case "difficulty":
                        switch (line.Substring(info.Length + 1).Trim())
                        {
                            case "None":
                                Diff = Difficulty.None;
                                break;
                            case "Fun":
                                Diff = Difficulty.Fun;
                                break;
                            case "Easy":
                                Diff = Difficulty.Easy;
                                break;
                            case "Normal":
                                Diff = Difficulty.Normal;
                                break;
                            case "Hard":
                                Diff = Difficulty.Hard;
                                break;
                            case "Powerful":
                                Diff = Difficulty.Powerful;
                                break;
                            case "So:Powerful":
                                Diff = Difficulty.SoPowerful;
                                break;

                        }
                        break;
                    case "bpm":
                        if (float.TryParse(line.Substring(info.Length + 1).Trim(), out float result2) && result2 >= 0)
                            BPM = result2;
                        else
                        {
                            BPM = 185;
                        }
                        break;
                    case "offset":
                        if (float.TryParse(line.Substring(info.Length + 1).Trim(), out float result3) && result3 >= 0)
                            Offset = result3;
                        else
                        {
                            Offset = 0;
                        }
                        break;
                    case "bg_offset":
                        if (float.TryParse(line.Substring(info.Length + 1).Trim(), out float result4) && result4 >= 0)
                            BGOffset = result4;
                        else
                        {
                            BGOffset = 0;
                        }
                        break;
                    default:
                        break;
                }
            }
            //Data
            Fraction current_segment_start_pos = new Fraction(0, 1);
            while (read_a_valid_line())
            {
                line = line.Trim();
                string[] info = line.Split(',');
                switch (info[0])
                {
                    case "D"://Hit
                        if (int.TryParse(info[1], out int bar1) &&
                            int.TryParse(info[2], out int numerator1) &&
                            int.TryParse(info[3], out int denominator1) &&
                            float.TryParse(info[4], out float track1) &&
                            int.TryParse(info[5], out int count1))
                        {
                            int remove_count = 0;
                            float scale = 1.0f;
                            int y_offset = 0;
                            if (info.Length >= 7)
                            {
                                int.TryParse(info[6], out remove_count);
                            }
                            if (info.Length >= 8)
                            {
                                float.TryParse(info[7], out scale);
                            }
                            if (info.Length >= 9)
                            {
                                int.TryParse(info[8], out y_offset);
                            }
                            if (denominator1 == 0)
                            { denominator1 = 1; numerator1 = 0; }
                            Fraction pos = new Fraction(numerator1 + bar1 * denominator1, denominator1);
                            Editor.Instance.PlaceHitNote(HitColor.Normal, pos + current_segment_start_pos, track1 - 1, count1, remove_count, false, scale, y_offset);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "X"://Gold
                        if (int.TryParse(info[1], out int bar2) &&
                            int.TryParse(info[2], out int numerator2) &&
                            int.TryParse(info[3], out int denominator2) &&
                            float.TryParse(info[4], out float track2) &&
                            int.TryParse(info[5], out int count2))
                        {
                            int remove_count = 0;
                            float scale = 1.0f;
                            int y_offset = 0;
                            if (info.Length >= 7)
                            {
                                int.TryParse(info[6], out remove_count);
                            }
                            if (info.Length >= 8)
                            {
                                float.TryParse(info[7], out scale);
                            }
                            if (info.Length >= 9)
                            {
                                int.TryParse(info[8], out y_offset);
                            }
                            if (denominator2 == 0)
                            { denominator2 = 1; numerator2 = 0; }
                            Fraction pos = new Fraction(numerator2 + bar2 * denominator2, denominator2);
                            Editor.Instance.PlaceHitNote(HitColor.Gold, pos + current_segment_start_pos, track2 - 1, count2, remove_count, false, scale, y_offset);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "B"://BPM
                        if (int.TryParse(info[1], out int bar3) &&
                            int.TryParse(info[2], out int numerator3) &&
                            int.TryParse(info[3], out int denominator3) &&
                            float.TryParse(info[4], out float bpm_value))
                        {
                            if (denominator3 == 0)
                            { denominator1 = 3; numerator3 = 0; }
                            Fraction pos = new Fraction(numerator3 + bar3 * denominator3, denominator3);
                            Editor.Instance.PlaceBPMNoteOrModify(pos + current_segment_start_pos, bpm_value, false);
                            current_segment_start_pos = pos + current_segment_start_pos;
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "H"://BKG
                        if (int.TryParse(info[1], out int bar4) &&
                            int.TryParse(info[2], out int numerator4) &&
                            int.TryParse(info[3], out int denominator4))
                        {
                            if (denominator4 == 0)
                            { denominator4 = 1; numerator4 = 0; }
                            Fraction pos = new Fraction(numerator4 + bar4 * denominator4, denominator4);
                            Editor.Instance.PlaceBKGNote(pos + current_segment_start_pos);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    default:
                        break;
                    case "S"://Performance
                        if (int.TryParse(info[1], out int bar5) &&
                            int.TryParse(info[2], out int numerator5) &&
                            int.TryParse(info[3], out int denominator5) &&
                            float.TryParse(info[4], out float track5) &&
                            int.TryParse(info[5], out int count5))
                        {
                            int remove_count = 0;
                            float scale = 1.0f;
                            int y_offset = 0;
                            if (info.Length >= 7)
                            {
                                int.TryParse(info[6], out remove_count);
                            }
                            if (info.Length >= 8)
                            {
                                float.TryParse(info[7], out scale);
                            }
                            if (info.Length >= 9)
                            {
                                int.TryParse(info[8], out y_offset);
                            }
                            if (denominator5 == 0)
                            { denominator5 = 1; numerator5 = 0; }
                            Fraction pos = new Fraction(numerator5 + bar5 * denominator5, denominator5);
                            Editor.Instance.PlaceHitNote(HitColor.Performance, pos + current_segment_start_pos, track5 - 1, count5, remove_count, false, scale, y_offset);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "C"://Camera
                        if (int.TryParse(info[1], out int bar6) &&
                            int.TryParse(info[2], out int numerator6) &&
                            int.TryParse(info[3], out int denominator6) &&
                            int.TryParse(info[4], out int easing) &&
                            int.TryParse(info[5], out int bar_duration)&&
                            int.TryParse(info[6], out int num_duration)&&
                            int.TryParse(info[7], out int den_duration)&&
                            int.TryParse(info[8], out int rot_z)&&
                            int.TryParse(info[9], out int rot_y)&&
                            int.TryParse(info[10], out int rot_x) &&
                            int.TryParse(info[11], out int pos_z) &&
                            int.TryParse(info[12], out int pos_y) &&
                            int.TryParse(info[13], out int pos_x)
                            )
                        {
                            easing--;
                            if (easing < 0 || easing >= (int)EasingType.NumEasingType) break;
                            if (denominator6 == 0)
                            { denominator6 = 1; numerator6 = 0; }
                            if (den_duration == 0)
                            { den_duration = 1; num_duration = 0; }
                            Fraction pos = new Fraction(numerator6 + bar6 * denominator6, denominator6);
                            Fraction duration = new Fraction(num_duration + bar_duration * den_duration, den_duration);
                            Editor.Instance.PlaceCameraNote(pos, duration,(EasingType)easing,new Vector3(pos_x,pos_y,pos_z),new Vector3(rot_x,rot_y,rot_z));
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                }
            }
            sr.Close();
        }
        else
        {
            sr.Close(); return false;
        }
        CommandSystem.ClearStack();
        GD.Print("FileProcesse:Line=Null,Read Ended!");

        string pathMP3 = Path.GetDirectoryName(path) + "\\music.mp3";
        if (File.Exists(pathMP3))
        {
            MediaManager.LoadBGMFromPath(pathMP3);
        }
        string pathWav = Path.GetDirectoryName(path) + "\\music.wav";
        if (File.Exists(pathWav))
        {
            MediaManager.LoadBGMFromPath(pathWav);
        }
        return true;
    }
}
