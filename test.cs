/// test.cs
/// Sample for using SMFPlayer
/// Copyright (c) 2025 gotojo, All Rights Reserved.

using System.IO;

namespace Test
{
	public class Test
	{
		static void Main(string[] args)
		{
			string mode = "p";
			string filename = "";
			if (args.Count() == 0)
			{
				Console.WriteLine("usage: test p|m filename");
				return;
			}
			else if (args.Count() == 1)
			{
				filename = args[0];
			}
			else
			{
				mode = args[0];
				filename = args[1];
			}
			Test test = new Test();
			if (mode == "m")
			{
				test.MapCreator(filename);
			}
			else
			{
				test.LyricPlayer(filename);
			}
		}
		void LyricPlayer(string filename)
		{
			string smfPath = filename;
			MidiWatcher midiWatcher = MidiWatcher.Instance;
			midiWatcher.onMidiIn += MIDIIn;
			midiWatcher.onLyricIn += LyricIn;
			midiWatcher.onTempoIn += TempoIn;
			midiWatcher.onBeatIn += BeatIn;
			midiWatcher.onMeasureIn += MeasureIn;
			midiWatcher.onEventIn += EventIn;
			SMFPlayer smfPlayer = new SMFPlayer(smfPath, midiWatcher);
			smfPlayer.Start();
			Task.Run(() =>
			{
				while (smfPlayer.Update())
				{
					Task.Delay(100);
				}
				;
			});
			while (smfPlayer.isPlaying())
			{
				Task.Delay(100);
			}
		}
		void MapCreator(string filename)
		{
			string smfPath = filename;
			MIDIEventMap map = new MIDIEventMap();
			SMFPlayer smfPlayer = new SMFPlayer(smfPath, map);
			map.Init(smfPlayer);
			var numOfMeasure = smfPlayer.numOfMeasure;
			var numOfTrack = smfPlayer.GetNumOfTrack();
			Console.WriteLine($"numOfTrack: {numOfTrack}");
			Console.WriteLine("each lyrics");
			Console.Write("measure");
			for (var track = 0; track < numOfTrack; track++)
			{
				Console.Write(", lyric, time, position");
			}
			Console.WriteLine();
			for (var meas = 0; meas < numOfMeasure; meas++)
			{
				Console.Write($"{meas}");
				for (var track = 0; track < numOfTrack; track++)
				{
					var numOfLyric = map.GetNumOfLyrics(meas, track);
					for (var i = 0; i < numOfLyric; i++)
					{
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
			Console.WriteLine("measure, time, lyrics");
			for (var meas = 0; meas < numOfMeasure; meas++)
			{
				UInt32 msec = 0;
				for (var track = 0; track < numOfTrack; track++)
				{
					if (!map.DataExist(meas, track, 0)) continue;
					msec = map.GetMsec(meas, track, 0);
					break;
				}
				Console.Write($"{meas}, {msec}");
				for (var track = 0; track < numOfTrack; track++)
				{
					var numOfLyric = map.GetNumOfLyrics(meas, track);
					string sentence = "";
					for (var i = 0; i < numOfLyric; i++)
					{
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
		public void EventIn(MIDIHandler.Event playerEvent)
		{
			string eventName = "";
			switch (playerEvent) {
			case MIDIHandler.Event.Start:
				eventName = "Start";
				break;
			case MIDIHandler.Event.Stop:
				eventName = "Stop";
				break;
			case MIDIHandler.Event.Reset:
				eventName = "Reset";
				break;
			case MIDIHandler.Event.End:
				eventName = "End";
				break;
			default:
				break;
			}
			Console.WriteLine($"Event:{eventName}");
		}
	}
}
