using System.Collections;
using System.Collections.Generic;
public class MidiWatcher : MIDIHandler
{
    Visualizer visualizer;
    public MidiWatcher(Visualizer visualizer)
    {
        this.visualizer = visualizer;
    }
    public override void MIDIIn(byte[] midiEvent)
    {
        visualizer.MIDIIn(midiEvent);
    }

    public override void LyricIn(string lyric)
    {
        visualizer.LyricIn(lyric);
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
    public void MIDIIn(byte[] midiEvent)
    {
    }
    public void LyricIn(string lyric)
    {
        Console.WriteLine(lyric);
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
