using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
public class SMFPlayer
{
	public static UInt32 tempo = 120;
	private bool isValid = false;
	private UInt16 format = 0;
	private UInt16 nTracks = 0;
	private List<Track> tracks = new List<Track>();
	private MIDIHandler midiHandler;
	private bool playing = false;

	public static UInt32 BEReader(BinaryReader reader, int len)
	{
		UInt32 value = 0;
		for (int i = 0; i < len; i++)
		{
			byte data = reader.ReadByte();
			value <<= 8;
			value += data;
		}
		return value;
	}
	public SMFPlayer(string filepath, MIDIHandler midiHandler)
	{
		if (string.IsNullOrEmpty(filepath)) {
			// Console.WriteLine("File path is null or empty.");
			return;
		}
		if (!File.Exists(filepath)) {
			// Console.WriteLine("File does not exist: " + filepath);
			return;
		}
		this.midiHandler = midiHandler;
		isValid = true;
		tracks.Clear();
		// Console.WriteLine("Loading SMF: " + filepath);
		// Console.WriteLine(filepath);
		using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
		{
			BinaryReader reader = new BinaryReader(fs);
			isValid = ParseChunk(reader);
		}
	}
	public void Reset()
	{
		foreach (Track track in tracks){
			track.Reset();
		}
	}
	public bool isPlay() {
		return this.playing;
	}
	private UInt32 tickup() {
		UInt32 nexttime = UInt32.MaxValue;
		foreach (Track track in tracks)
		{
			if (track.isEnd)
			{
				continue;
			}
			UInt32 _nexttime = track.Tickup();
			if (_nexttime < nexttime)
			{
				nexttime = _nexttime;
			}
		}
		return nexttime;
	}
	public bool Play()
	{
		if (!isValid) {
			return false;
		}
		playing = true;
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		UInt32 startTime = (UInt32)stopWatch.ElapsedMilliseconds;
		Reset();
		UInt32 nexttime = tickup();
		if (nexttime == UInt32.MaxValue) {
			playing = false;
		}
		UInt32 nextEventTime = nexttime / 1000 + startTime;
		// Console.WriteLine($"nextEventTime: {nextEventTime}");
		Task.Run(async () =>
		{
			while (playing) {
				UInt32 currentTime = (UInt32)stopWatch.ElapsedMilliseconds;
				// Console.WriteLine($"currentTime: {currentTime}");
				while (currentTime >= nextEventTime) {
					nexttime = tickup();
					if (nexttime == UInt32.MaxValue) {
						playing = false;
						break;
					}
					nextEventTime = nexttime / 1000 + startTime;
					// Console.WriteLine($"currentTime: {currentTime}, nextEventTime: {nextEventTime}");
				}
				await Task.Delay(100);
			};
			stopWatch.Stop();
		});
		return true;
	}
	public bool Stop()
	{
		if (!isValid) {
			return false;
		}
		return true;
	}

	private bool ParseChunk(BinaryReader reader) 
	{
		char[] type = new char[4];
		int trackid = 0;
		do {
			reader.Read(type, 0, 4);
			string typeString = new string(type);
			switch (typeString) {
			case "MThd":
				if (!ParseHeader(reader)) {
					return false;
				}
				break;
			case "MTrk":
				// Console.WriteLine("Track Chunk");
				Track track = new Track(trackid, reader, this.midiHandler);
				if (track.isValid) {
					tracks.Add(track);
					trackid++;
				}
				break;
			default:
				// Console.WriteLine("Unknown Chunk");
				UInt32 size = BEReader(reader, 4);
				reader.BaseStream.Seek(size, SeekOrigin.Current);
				break;
			}
		} while (reader.BaseStream.Position < reader.BaseStream.Length);
		return true;
	}

	private bool ParseHeader(BinaryReader reader)
	{
		UInt32 size = BEReader(reader, 4);
		if (size != 6) {
			return false;
		}
		format = (UInt16)BEReader(reader, 2);
		nTracks = (UInt16)BEReader(reader, 2);
		Track.tpqn = (UInt16)BEReader(reader, 2);
		return true;
	}

	class Track {
		public int id;
		public UInt32 size;
		public bool isValid = true;
		public bool isEnd = false;
		public static UInt32 tpqn = 96;
		private byte runningStatus = 0;
		private UInt32 currentTick = 0;
		private int currentEventIndex = 0;
		private UInt32 nextEventTick = 0;
		private UInt32 nextEventUsec = 0;
		private static UInt32 usecPerQuarterNote = 60000000 / 120;

		private struct MIDIEvent {
			public UInt32	deltaTime;
			public byte[]	data;
			public UInt32	usec;
		};
		private List<MIDIEvent> midiEvents = new List<MIDIEvent>();
		private MIDIHandler midiHandler;

		public Track(int num, BinaryReader reader, MIDIHandler handler) {
			id = num;
			midiHandler = handler;
			Reset();
			UInt32 size = SMFPlayer.BEReader(reader, 4);
			this.size = size;
			if (size == 0) {
				isValid = false;
				return;
			}
			byte[] data = new byte[size];
			reader.Read(data, 0, (int)size);
			long endPosition = reader.BaseStream.Position + size;
			BinaryReader bufferReader = new BinaryReader(new MemoryStream(data));
			ParseBody(bufferReader, endPosition);
		}
		public void Reset() {
			isEnd = false;
			currentEventIndex = 0;
			nextEventTick = (midiEvents.Count > 0) ? midiEvents[currentEventIndex].deltaTime : 0;
			nextEventUsec = 0;
			runningStatus = 0;
			currentTick = 0;
		}
		private void ParseBody(BinaryReader reader, long endPosition) {
			UInt32 time = 0;
			while (reader.BaseStream.Position < endPosition) {
				UInt32 deltaTime = ParseDeltaTime(reader);
				// Console.WriteLine($"deltaTime: {deltaTime}");
				byte[] eventData = ParseEvent(reader);
				MIDIEvent midiEvent = new MIDIEvent();
				midiEvent.deltaTime = deltaTime;
				midiEvent.data = eventData;
				time += deltaTime * Track.usecPerQuarterNote / tpqn;
				midiEvent.usec = time;
				midiEvents.Add(midiEvent);
				if (isEnd) {
					break;
				}
			}
		}
		private UInt32 ParseDeltaTime(BinaryReader reader) {
			return ParseVariableLength(reader);
		}
		private UInt32 ParseVariableLength(BinaryReader reader) {
			UInt32 value = 0;
			byte b;
			do {
				b = reader.ReadByte();
				value = (value << 7) | (UInt32)(b & 0x7F);
			} while ((b & 0x80) != 0);
			return value;
		}
		private byte[] ParseEvent(BinaryReader reader) {
			byte status = reader.ReadByte();
			if (status < 0x80) {
				// Running Status
				reader.BaseStream.Seek(-1, SeekOrigin.Current);
				status = runningStatus;
			} else {
				runningStatus = status;
			}
			switch (status) {
			case 0xFF:
				// Meta Event
				byte metaType = reader.ReadByte();
				UInt32 size = ParseVariableLength(reader);
				byte[] metaData = new byte[size + 2];
				metaData[0] = status;
				metaData[1] = metaType;
				reader.Read(metaData, 2, (int)size);
				if (metaType == 0x2F) {
					// End of Track
					isEnd = true;
				} else if (metaType == 0x51) {
					UInt32 usecPerQuarterNote = (UInt32)(metaData[2] << 16 | metaData[3] << 8 | metaData[4]);
					usecPerQuarterNote &= 0x00FFFFFF;
					SMFPlayer.tempo = 60000000 / usecPerQuarterNote;
					Track.usecPerQuarterNote = usecPerQuarterNote;
					// Console.WriteLine("Tempo: " + tempo);
				}
				return metaData;
			case 0xF0:
			case 0xF7:
				// SysEx Event
				UInt32 sysexSize = ParseVariableLength(reader);
				byte[] sysexData = new byte[sysexSize + 1];
				sysexData[0] = status;
				reader.Read(sysexData, 1, (int)sysexSize);
				return sysexData;
			case 0x80:
			case 0x90:
			case 0xE0:
				// 3byte events
				byte[] data3 = new byte[3];
				data3[0] = status;
				data3[1] = reader.ReadByte();
				data3[2] = reader.ReadByte();
				return data3;
			case 0xA0:
			case 0xB0:
			case 0xC0:
			case 0xD0:
				// 2byte events
				byte[] data2 = new byte[2];
				data2[0] = status;
				data2[1] = reader.ReadByte();
				return data2;
			default:
				// Unknown event
				// Console.WriteLine("Unknown Event");
				byte[] unknownData = new byte[1];
				unknownData[0] = status;
				return unknownData;
			}
		}
		public UInt32 Tickup()
		{
			if (isEnd) {
				return uint.MaxValue;
			}
			if (currentEventIndex >= midiEvents.Count) {
				return uint.MaxValue;
			}
			while (currentTick >= nextEventTick) {
				MIDIEvent _midiEvent = midiEvents[currentEventIndex];
				// Console.WriteLine($"usec:{_midiEvent.usec}");
				DoMIDIEvent(_midiEvent.data);
				currentEventIndex++;
				if (currentEventIndex < midiEvents.Count) {
					nextEventTick = currentTick + midiEvents[currentEventIndex].deltaTime;
					nextEventUsec = midiEvents[currentEventIndex].usec;
					// Console.WriteLine($"currentTick: {currentTick}, nextEventTick:{nextEventTick}, usec:{_midiEvent.usec}");
				}
				else {
					nextEventTick = uint.MaxValue;
					nextEventUsec = uint.MaxValue;
					isEnd = true;
					break;
				}
			}
			currentTick++;
			return nextEventUsec;
		}
		private string GetMetaText(byte[] data) {
			String text = System.Text.Encoding.UTF8.GetString(data, 2, data.Length - 2);
			return text;
		}
		private void DoMIDIEvent(byte[] data)
		{
			if (data[0] == 0xFF) {
				// Meta Event
				switch (data[1])
				{
					case 0x51:
						// Set Tempo
						UInt32 usecPerQuarterNote = (UInt32)(data[2] << 16 | data[3] << 8 | data[4]);
						usecPerQuarterNote &= 0x00FFFFFF;
						SMFPlayer.tempo = 60000000 / usecPerQuarterNote;
						Track.usecPerQuarterNote = usecPerQuarterNote;
						// Console.WriteLine("Tempo: " + tempo);
						break;
					case 0x2F:
						// End of Track
						// Console.WriteLine("End of Track");
						isEnd = true;
						break;
					case 0x5:
						//Lyric Event
						midiHandler.LyricIn(GetMetaText(data));
						break;
					default:
						// Console.WriteLine("Meta Event: " + data[1]);
						break;
				}
			} else {
				// MIDI Event
				// Console.WriteLine("MIDI Event: " + data[0]);
				midiHandler.MIDIIn(data);
			}
		}
	}

}
