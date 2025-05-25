///
///	MidiWatcher
/// 
/// Copyright (c) gotojo, All Rights Reserved.
/// 

using System;

public class MidiWatcher : MIDIHandler
{
	private static MidiWatcher? _instance;  // singleton
	public static MidiWatcher Instance
	{
		get {
			if (_instance == null) {
				_instance = new MidiWatcher();
			}
			return _instance;
		}
	}
	public delegate void midiInHandler(int track, byte[] midiEvent, float position, uint currentMsec);
	public delegate void lyricInHandler(int track, string lyric, float position, uint currentMsec);
	public delegate void tempoInHandler(float msecPerQuaterNote, uint tempo, uint currentMsec);
	public delegate void beatInHandler(int numerator, int denominator, uint currentMsec);
	public delegate void measureInHandler(int measure, int measureInterval, uint currentMsec);
	public delegate void eventInHandler(MIDIHandler.Event playerEvent);
	public event midiInHandler? 		onMidiIn;
	public event lyricInHandler? 	onLyricIn;
	public event tempoInHandler? 	onTempoIn;
	public event beatInHandler? 		onBeatIn;
	public event measureInHandler? 	onMeasureIn;

	private MidiWatcher()
	{
	}

	public void Clear()
	{
		onMidiIn = null;
		onLyricIn = null;
		onTempoIn = null;
		onBeatIn = null;
		onMeasureIn = null;
		onEventIn = null;
	}

	public override void MIDIIn(int track, byte[] midiEvent, float position, uint currentMsec)
	{
		onMidiIn?.Invoke(track, midiEvent, position, currentMsec);
	}
	public override void LyricIn(int track, string lyric, float position, uint currentMsec)
	{
		// Console.WriteLine(lyric);
		onLyricIn?.Invoke(track, lyric, position, currentMsec);
	}
	public override void TempoIn(float msecPerQuaterNote, uint tempo, uint currentMsec)
	{
		onTempoIn?.Invoke(msecPerQuaterNote, tempo, currentMsec);
	}
	public override void BeatIn(int numerator, int denominator, uint currentMsec)
	{
		// Console.WriteLine($"BeatIn: {numerator}/{denominator}");
		onBeatIn?.Invoke(numerator, denominator, currentMsec);
	}
	public override void MeasureIn(int measure, int measureInterval, uint currentMsec)
	{
		// Console.WriteLine($"MeasureIn: Measure: {measure}, Interval: {measureInterval}");
		onMeasureIn?.Invoke(measure, measureInterval, currentMsec);
	}
}
