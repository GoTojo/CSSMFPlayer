/// test.cs
/// Sample for using SMFPlayer
/// Copyright (c) 2025 gotojo, All Rights Reserved.

using System.IO;

namespace Test
{
	public class Test
	{
		static void Main()
		{
			Test test = new Test();
			Console.WriteLine("Hello, World!");
			// string smfPath = @"らーめん食べよう.mid";
			string smfPath = @"3分間のトキメキ.mid";
			MidiWatcher midiWatcher = new MidiWatcher();
			midiWatcher.onMidiIn += test.MIDIIn;
			midiWatcher.onLyricIn += test.LyricIn;
			midiWatcher.onTempoIn += test.TempoIn;
			midiWatcher.onBeatIn += test.BeatIn;
			midiWatcher.onMeasureIn += test.MeasureIn;
			SMFPlayer smfPlayer = new SMFPlayer(smfPath, midiWatcher);
			smfPlayer.Start();
			Task.Run(() =>
			{
				while(smfPlayer.Update()) {
					Task.Delay(100);
				} ;
			});
			while (smfPlayer.isPlaying()) {
				Task.Delay(100);
			}
		}
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
};
