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
			// test.LyricPlayer();
			test.MapCreator();
		}
		void LyricPlayer()
		{
			// string smfPath = @"らーめん食べよう.mid";
			string smfPath = @"3分間のトキメキ.mid";
			MidiWatcher midiWatcher = new MidiWatcher();
			midiWatcher.onMidiIn += MIDIIn;
			midiWatcher.onLyricIn += LyricIn;
			midiWatcher.onTempoIn += TempoIn;
			midiWatcher.onBeatIn += BeatIn;
			midiWatcher.onMeasureIn += MeasureIn;
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
		void MapCreator()
		{
			Console.WriteLine("Hello, World!");
			// string smfPath = @"らーめん食べよう.mid";
			string smfPath = @"3分間のトキメキ.mid";
			MIDIEventMap map = new MIDIEventMap();
			SMFPlayer smfPlayer = new SMFPlayer(smfPath, map);
			map.Init(smfPlayer);
			var numOfMeasure = smfPlayer.numOfMeasure;
			var numOfTrack = smfPlayer.GetNumOfTrack();
			Console.WriteLine($"numOfTrack: {numOfTrack}");
			Console.WriteLine("each lyrics");
			Console.Write("measure");
			for (var track = 0; track < numOfTrack; track++) {
				Console.Write(", lyric, time, position");
			}
			Console.WriteLine();
			for (var meas = 0; meas < numOfMeasure; meas++) {
				Console.Write($"{meas}");
				for (var track = 0; track < numOfTrack; track++) {
					var numOfLyric = map.GetNumOfLyrics(meas, track);
					for (var i = 0; i < numOfLyric; i++) {
						string lyric = map.GetLyric(meas, track, i);
						UInt32 msec = map.GetMsec(meas, track, i);
						float position = map.GetPosition(meas, track, i);
						Console.Write($", {lyric}, {msec}, {position}");
					}
				}
				Console.WriteLine();
			}
			Console.WriteLine("-----------------------------------------");
			Console.WriteLine("each measure");
			Console.WriteLine("measure, lyrics, time, position");
			for (var meas = 0; meas < numOfMeasure; meas++) {
				UInt32 msec = 0;
				for (var track = 0; track < numOfTrack; track++) {
					if (!map.DataExist(meas, track, 0)) continue;
					msec = map.GetMsec(meas, track, 0);
					break;
				}
				Console.Write($"{meas}, {msec}");
				for (var track = 0; track < numOfTrack; track++) {
					var numOfLyric = map.GetNumOfLyrics(meas, track);
					string sentence = "";
					for (var i = 0; i < numOfLyric; i++) {
						string lyric = map.GetLyric(meas, track, i);
						sentence += lyric;
					}
					Console.Write($", {sentence}");
				}
				Console.WriteLine();
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
}
