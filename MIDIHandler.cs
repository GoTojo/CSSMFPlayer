/// MIDIHandler.cs
/// Interface Class for Receive MidiEvents
/// Copyright (c) 2025 gotojo, All Rights Reserved.

public class MIDIHandler {
	public virtual void MIDIIn(byte[] midiEvent, float position) {

	}
	public virtual void LyricIn(string lyric, float position) {
		
	}
	public virtual void BeatIn(int numerator, int denominator) {
		
	}
	public virtual void MeasureIn(int measure) {
		
	}
}