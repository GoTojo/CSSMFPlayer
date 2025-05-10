/// Visualizer.cs
/// Concrete Class for SMFPlayer
/// Copyright (C) 2025 gotojo, All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
public class MidiWatcher : MIDIHandler
{
    Visualizer visualizer;
    public MidiWatcher(Visualizer visualizer)
    {
        this.visualizer = visualizer;
    }
    public override void MIDIIn(int track, byte[] midiEvent, float position, UInt32 currentMsec)
    {
        visualizer.MIDIIn(track, midiEvent, position, currentMsec);
    }

    public override void LyricIn(int track, string lyric, float position, UInt32 currentMsec)
    {
        visualizer.LyricIn(track, lyric, position, currentMsec);
    }
    public override void TempoIn(float msecPerQuaterNote, UInt32 tempo, UInt32 currentMsec)
    {
        visualizer.TempoIn(msecPerQuaterNote, tempo, currentMsec);
    }
    public override void BeatIn(int numerator, int denominator, UInt32 currentMsec)
    {
        visualizer.BeatIn(numerator, denominator, currentMsec);
    }

    public override void MeasureIn(int num, int measureInterval, UInt32 currentMsec)
    {
        visualizer.MeasureIn(num, measureInterval, currentMsec);
    }
}
public class Visualizer
{
    public void MIDIIn(int track, byte[] midiEvent, float position, UInt32 currentMsec)
    {
        Console.WriteLine($"Track: {track}, status: {midiEvent[0]}, position: {position}, currentMsec: {currentMsec}");
    }
    public void LyricIn(int track, string lyric, float position, UInt32 currentMsec)
    {
        Console.WriteLine($"Track: {track}, lyric: {lyric}, position: {position}, currentMsec: {currentMsec}");
    }
    public void TempoIn(float msecPerQuaterNote, UInt32 tempo, UInt32 currentMsec)
    {
        Console.WriteLine($"tempo: {(int)msecPerQuaterNote}ms({tempo}), currentMsec: {currentMsec}");
    }
    public void BeatIn(int numerator, int denominator, UInt32 currentMsec)
    {
        Console.WriteLine($"Beat: {numerator} / {denominator}, currentMsec: {currentMsec}");
    }
    public void MeasureIn(int num, int measureInterval, UInt32 currentMsec)
    {
        Console.WriteLine($"Measure: {num}, Interval: {measureInterval}, currentMsec: {currentMsec}");
    }
}
