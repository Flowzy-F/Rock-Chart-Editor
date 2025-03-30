using Godot;
using System;
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
        sw.WriteLine($"level={Level}");
        sw.WriteLine($"bpm={BPM}");
        sw.WriteLine($"offset={Offset}");
        sw.WriteLine($"bg_offset={BGOffset}");

        sw.WriteLine("[Data]");
        Dictionary<NoteHash, NoteData> ordered_datas = NoteMap.OrderBy(n => (float)n.Value.Position.Numerator / n.Value.Position.Denominator).ToDictionary();
        Fraction current_segment_start_pos = new Fraction(0, 1);
        foreach (NoteData d in ordered_datas.Values)
        {
            Fraction segment_local_pos = d.Position - current_segment_start_pos;
            if (HitnoteTypes.Contains(d.Type))
            {
                char type_char = ' ';
                if (d.Type == NoteType.Normal) type_char = 'D';
                else if (d.Type == NoteType.Gold) type_char = 'X';
                sw.WriteLine($"{type_char},{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator},{d.Track + 1},{((HitNoteData)d).Count},{((HitNoteData)d).RemoveCount}," +
                    $"{((HitNoteData)d).Scale},{((HitNoteData)d).YOffset}");

            }
            else if (d.Type == NoteType.BPM)
            {
                sw.WriteLine($"B,{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator},{d.Track + 1},{((BPMNoteData)d).BPMValue}");
                current_segment_start_pos = d.Position;
            }
            else if (d.Type == NoteType.BKG)
            {
                sw.WriteLine($"H,{segment_local_pos.GetWhole()},{segment_local_pos.GetTrueNumerator()}," +
                    $"{segment_local_pos.Denominator},{d.Track + 1}");
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
                    case "level":
                        if (int.TryParse(line.Substring(info.Length+1).Trim(), out int result1) && result1 >= 0)
                            Level = result1;
                        else
                        {
                            Level = 0;
                        }
                        break;
                    case "bpm":
                        if (float.TryParse(line.Substring(info.Length+1).Trim(), out float result2) && result2 >= 0)
                            BPM = result2;
                        else
                        {
                            BPM = 185;
                        }
                        break;
                    case "offset":
                        if (float.TryParse(line.Substring(info.Length+1).Trim(), out float result3) && result3 >= 0)
                            Offset = result3;
                        else
                        {
                            Offset = 0;
                        }
                        break;
                    case "bg_offset":
                        if (float.TryParse(line.Substring(info.Length+1).Trim(), out float result4) && result4 >= 0)
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
                    case "D":
                        if (int.TryParse(info[1], out int bar1) &&
                            int.TryParse(info[2], out int numerator1) &&
                            int.TryParse(info[3], out int denominator1) &&
                            int.TryParse(info[4], out int track1) &&
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
                            if (denominator1==0)
                            { denominator1 = 1;numerator1 = 0; }
                            Fraction pos = new Fraction(numerator1 + bar1 * denominator1, denominator1);
                            Editor.Instance.PlaceHitNote(NoteType.Normal, pos + current_segment_start_pos, track1 - 1, count1, remove_count, false, scale,y_offset);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "X":
                        if (int.TryParse(info[1], out int bar2) &&
                            int.TryParse(info[2], out int numerator2) &&
                            int.TryParse(info[3], out int denominator2) &&
                            int.TryParse(info[4], out int track2) &&
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
                            if (denominator2==0)
                            { denominator2 = 1;numerator2 = 0; }
                            Fraction pos = new Fraction(numerator2 + bar2 * denominator2, denominator2);
                            Editor.Instance.PlaceHitNote(NoteType.Gold, pos + current_segment_start_pos, track2 - 1, count2, remove_count, false, scale,y_offset);
                        }
                        else
                        {
                            //TODO
                        }
                        break;
                    case "B":
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
                    case "H":
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
        return true;
    }
}
