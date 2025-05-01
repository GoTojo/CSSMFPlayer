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
    public override void MIDIIn(byte[] midiEvent, float position)
    {
        visualizer.MIDIIn(midiEvent, position);
    }

    public override void LyricIn(string lyric, float position)
    {
        visualizer.LyricIn(lyric, position);
    }
 
    public override void BeatIn(int numerator, int denominator)
    {
        visualizer.BeatIn(numerator, denominator);
    }

    public override void MeasureIn(int num)
    {
        visualizer.MeasureIn(num);
    }
}
public class Visualizer
{
    public void MIDIIn(byte[] midiEvent, float position)
    {
        Console.WriteLine($"status: {midiEvent}, position: {position}");
    }
    public void LyricIn(string lyric, float position)
    {
        Console.WriteLine($"lyric: {lyric}, position: {position}");
    }
    public void BeatIn(int numerator, int denominator)
    {
        Console.WriteLine($"Beat: {numerator} / {denominator}");
    }
    public void MeasureIn(int num)
    {
        Console.WriteLine($"Measure: {num}");
    }
}
