using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Editor;
using static Utils;
public partial class SyncTimeSystem : Node
{
    public override void _Ready()
    {
        BuildBPMTimeLine();
        base._Ready();
    }
    private static List<BPMEvent> bpmEvents = new List<BPMEvent>();
    public static void BuildBPMTimeLine()
    {
        bpmEvents.Clear();
        List<NoteInfo.NoteHash> ordered_bpm_list = BPMNoteList.OrderBy(n => ((double)n.Position.Numerator / n.Position.Denominator)).ToList();
        double current_time = 0;
        //Add a bpm event at 0:0/0 as the base BPM.
        BPMEvent start_e = new BPMEvent() { BPMValue = BPM, BarPosition = 0, StartTime = current_time };
        bpmEvents.Add(start_e);
        foreach (var h in ordered_bpm_list)
        {
            BPMEvent last_e = bpmEvents.LastOrDefault();
            BPMEvent e = new BPMEvent();
            e.BarPosition = (double)(h.Position.Numerator) / h.Position.Denominator;
            if (NoteMap.TryGetValue(h, out var d))
                e.BPMValue = ((NoteInfo.BPMNoteData)d).BPMValue;
            else
                throw new Exception("BPM Note isn't found in NoteMap!");

            double past_fraction = e.BarPosition - last_e.BarPosition;
            double past_time = past_fraction * (60.0 / last_e.BPMValue);
            current_time += past_time;
            e.StartTime = current_time;
            bpmEvents.Add(e);
        }

        GD.Print("BPM Timeline (Re)built!");
    }
    public static float ToBarPosition(float time_s)
    {
        BPMEvent current_e = bpmEvents[BinaryFindCurrentBPMIndexByTime(time_s)];
        return (float)(
            current_e.BarPosition + (time_s - current_e.StartTime) / (60.0f / current_e.BPMValue)
            );
    }
    public static float ToTime(float bar_position)
    {
        BPMEvent current_e = bpmEvents[BinaryFindCurrentBPMIndexByBar(bar_position)];
        return (float)(
            current_e.StartTime + (TimeOffset + Bar - current_e.BarPosition) * (60.0f / current_e.BPMValue)
            );
    }

    private static int BinaryFindCurrentBPMIndexByTime(double time_s)
    {
        if (time_s <= 0) return 0;
        int low = 0;
        int high = bpmEvents.Count - 1;
        int mid = -1;
        int result = -1;
        while (low <= high)
        {
            mid = (low + high) / 2;
            if (bpmEvents[mid].StartTime <= time_s)
            {
                result = mid;
                low = mid + 1;
            }
            else high = mid - 1;
        }
        return result;
    }
    private static int BinaryFindCurrentBPMIndexByBar(double bar_pos)
    {
        if (bar_pos <= 0) return 0;
        int low = 0;
        int high = bpmEvents.Count - 1;
        int mid = -1;
        int result = -1;
        while (low <= high)
        {
            mid = (low + high) / 2;
            if (bpmEvents[mid].BarPosition <= bar_pos)
            {
                result = mid;
                low = mid + 1;
            }
            else high = mid - 1;
        }

        return result;
    }


    private struct BPMEvent
    {
        public double StartTime;
        public double BarPosition;
        public float BPMValue;
    }
}
