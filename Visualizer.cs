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
    public override void MIDIIn(int track, byte[] midiEvent, float position)
    {
        visualizer.MIDIIn(track, midiEvent, position);
    }

    public override void LyricIn(int track, string lyric, float position)
    {
        visualizer.LyricIn(track, lyric, position);
    }
    public override void TempoIn(float msecPerQuaterNote, UInt32 tempo)
    {
        visualizer.TempoIn(msecPerQuaterNote, tempo);
    }
    public override void BeatIn(int numerator, int denominator)
    {
        visualizer.BeatIn(numerator, denominator);
    }

    public override void MeasureIn(int num, int measureInterval)
    {
        visualizer.MeasureIn(num, measureInterval);
    }
}
public class Visualizer
{
    public void MIDIIn(int track, byte[] midiEvent, float position)
    {
        Console.WriteLine($"Track: {track}, status: {midiEvent[0]}, position: {position}");
    }
    public void LyricIn(int track, string lyric, float position)
    {
        Console.WriteLine($"Track: {track}, lyric: {lyric}, position: {position}");
    }
    public void TempoIn(float msecPerQuaterNote, UInt32 tempo)
    {
        Console.WriteLine($"tempo: {(int)msecPerQuaterNote}ms({tempo})");
    }
    public void BeatIn(int numerator, int denominator)
    {
        Console.WriteLine($"Beat: {numerator} / {denominator}");
    }
    public void MeasureIn(int num, int measureInterval)
    {
        Console.WriteLine($"Measure: {num}, Interval: {measureInterval}");
    }
}
