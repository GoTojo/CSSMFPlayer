///	
///	MIDIEventMap
/// copyright (c) 2025 gotojo, All Rights Reserved
///
using System;
using System.Collections.Generic;

public class MIDIEventMap : MIDIHandler
{
	public struct LyricData
	{
		public string lyric;
		public float position;
		public UInt32 msec;
		public LyricData(string lyric, float position, UInt32 msec)
		{
			this.lyric = lyric;
			this.position = position;
			this.msec = msec;
		}
	};
	List<List<List<LyricData>>> lyrics = new List<List<List<LyricData>>>();
	private int currentMeasure = 0;

	public MIDIEventMap()
	{
	}
	public void Init(SMFPlayer player)
	{
		int numOfMeasure = player.numOfMeasure;
		int numOfTrack = player.GetNumOfTrack();
		for (int meas = 0; meas < numOfMeasure; meas++) {
			var row = new List<List<LyricData>>(); // 2次元目を格納するList
			for (int track = 0; track < numOfTrack; track++) {
				row.Add(new List<LyricData>()); // 空の3次元目を追加
			}
			lyrics.Add(row); // 1次元目に追加
		}		
		player.Reset();
		player.Start();
		uint time = 0;
		while (player.Update(time)) {
			time += 10;
		}
		player.Reset();
		Reset();
	}
	public void Reset()
	{
		currentMeasure = 0;
	}

	public override void MIDIIn(int track, byte[] midiEvent, float position, UInt32 currentMsec)
	{
	}
	public override void LyricIn(int track, string lyric, float position, UInt32 currentMsec)
	{
		if (lyrics == null) {
			return;
		}
		if (currentMeasure >= lyrics.Count) {
			return;
		}
		LyricData data = new LyricData(lyric, position, currentMsec);
		// Debug.Log($"EventMap.LyricIn: currentMeasure:{currentMeasure}");
		lyrics[currentMeasure][track].Add(data);
	}
	public override void TempoIn(float msecPerQuaterNote, uint tempo, UInt32 currentMsec)
	{
	}
	public override void BeatIn(int numerator, int denominator, UInt32 currentMsec)
	{
	}
	public override void MeasureIn(int measure, int measureInterval, UInt32 currentMsec)
	{
		if (lyrics == null) {
			return;
		}
		if (measure < 0) {
			return;
		}
		currentMeasure = measure - 1;
	}

	public int GetNumOfLyrics(int measure, int track)
	{
		return lyrics[measure][track].Count;
	}
	public bool DataExist(int measure, int track, int num)
	{
		if (measure < lyrics.Count) {
			if (track < lyrics[measure].Count) {
				if (num < lyrics[measure][track].Count) {
					return true;
				}
			}
		}
		return false;
	}
	public LyricData GetLyricData(int measure, int track, int num)
	{
		if (measure < lyrics.Count) {
			if (track < lyrics[measure].Count) {
				if (num < lyrics[measure][track].Count) {
					return lyrics[measure][track][num];
				}
			}
		}
		LyricData data = new LyricData();
		return data;
	}
	public string GetLyric(int measure, int track, int num)
	{
		LyricData lyricData = GetLyricData(measure, track, num);
		return lyricData.lyric;
	}
	public float GetPosition(int measure, int track, int num)
	{
		LyricData lyricData = GetLyricData(measure, track, num);
		return lyricData.position;
	}
	public UInt32 GetMsec(int measure, int track, int num)
	{
		LyricData lyricData = GetLyricData(measure, track, num);
		return lyricData.msec;
	}
}
