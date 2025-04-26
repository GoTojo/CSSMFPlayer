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
}
