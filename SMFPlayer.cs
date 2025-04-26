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
	private UInt16 format = 0;
	public UInt32 tempo = 120;
	public UInt32 tpqn = 96;
	public float usecPerQuarterNote = 60000000 / 120;
	public bool isValid = false;
	private UInt16 nTracks = 0;
	private List<TrackData> tracks = new List<TrackData>();
	private List<TrackPlayer> players = new List<TrackPlayer>();
	private MIDIHandler midiHandler;
	private bool playing = false;
	public struct Beat
	{
		public int unit;
		public int count;
	};
	public Beat beat;

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
		players.Clear();
		// Console.WriteLine("Loading SMF: " + filepath);
		// Console.WriteLine(filepath);
		using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
		{
			BinaryReader reader = new BinaryReader(fs);
			isValid = ParseChunk(reader);
			foreach (TrackData track in tracks) {
				TrackPlayer player = new TrackPlayer(track, this, midiHandler);
				players.Add(player);
			}
		}
		// Console.WriteLine("complete parsing SMF");
	}
	public void Reset()
	{
		beat.count = 4;
		beat.unit = 4; // default is 4 / 4
		foreach (TrackPlayer player in players){
			player.Reset();
		}
	}
	public bool isPlaying() {
		return this.playing;
	}
	private UInt32 tickup() {
		UInt32 nexttime = UInt32.MaxValue;
		foreach (TrackPlayer player in players) {
			if (player.isEnd) {
				continue;
			}
			UInt32 _nexttime = player.Tickup();
			if (_nexttime < nexttime) {
				nexttime = _nexttime;
			}
		}
		return nexttime;
	}
	public bool Play()
	{
		// Console.WriteLine("Play Start");
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
		UInt32 nextEventTime = nexttime + startTime;
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
					nextEventTime = nexttime + startTime;
					// Console.WriteLine($"currentTime: {currentTime}, nextEventTime: {nextEventTime}");
				}
				UInt32 delay =(UInt32)(usecPerQuarterNote / 1000 / 4);
				// Console.WriteLine($"wait {delay}ms");
				await Task.Delay((int)delay);
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
				TrackParser parser = new TrackParser(trackid, reader, this);
				if (parser.isValid) {
					tracks.Add(parser);
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
		tpqn = (UInt16)BEReader(reader, 2);
		return true;
	}

	class TrackData {
		private int id;
		private SMFPlayer player;
		private bool isEnd;
		public struct MIDIEvent
		{
			public UInt32 deltaTime;
			public byte[] data;
			public UInt32 msec;
		};
		private List<MIDIEvent> midiEvents = new List<MIDIEvent>();
		private UInt32 currentEventIndex = 0;
		private UInt32 currentTime = 0;

		public TrackData(int trackid, SMFPlayer player)
		{
			id = trackid;
			this.player = player;
			Reset();
		}
		public void Clear()
		{
			Reset();
			midiEvents.Clear();
		}
		public bool Add(UInt32 deltaTime, byte[] data)
		{
			UInt32 deltaMSec = (UInt32)(deltaTime * player.usecPerQuarterNote / player.tpqn / 1000);
			MIDIEvent midiEvent = new MIDIEvent();
			if (deltaMSec >= (UInt32.MaxValue - currentTime)) {
				midiEvent.deltaTime = UInt32.MaxValue;
				midiEvent.data = null;
				midiEvent.msec = UInt32.MaxValue;
				midiEvents.Add(midiEvent);
				return false;
			}
			midiEvent.deltaTime = deltaTime;
			midiEvent.data = data;
			currentTime += deltaMSec;
			midiEvent.msec = currentTime;
			midiEvents.Add(midiEvent);
			return true;
		}
		public void Reset() {
			currentEventIndex = 0;
			currentTime = 0;
			isEnd = false;
		}
		public bool Next() {
			UInt32 index = currentEventIndex + 1;
			if (index >= midiEvents.Count) {
				isEnd = true;
				return false;
			}
			currentEventIndex = index;
			return true;
		}
		public UInt32 GetDeltaTime()
		{
			return midiEvents[(int)currentEventIndex].deltaTime;
		}
		public byte[] GetData()
		{
			return midiEvents[(int)currentEventIndex].data;
		}
		public UInt32 GetMsec()
		{
			return midiEvents[(int)currentEventIndex].msec;
		}
		public bool IsEnd()
		{
			return isEnd;
		}
	};

	class TrackParser : TrackData {
		public bool isValid = true;
		public bool isEnd = false;
		private SMFPlayer smfPlayer;
		private byte runningStatus = 0;
		public TrackParser(int trackid, BinaryReader reader, SMFPlayer player):base(trackid, player) {
			smfPlayer = player;
			runningStatus = 0;
			UInt32 size = SMFPlayer.BEReader(reader, 4);
			if (size == 0) {
				isValid = false;
				return;
			}
			Clear();
			byte[] data = new byte[size];
			reader.Read(data, 0, (int)size);
			long endPosition = reader.BaseStream.Position + size;
			BinaryReader bufferReader = new BinaryReader(new MemoryStream(data));
			ParseBody(bufferReader, endPosition);
		}
		private void ParseBody(BinaryReader reader, long endPosition)
		{
			while (reader.BaseStream.Position < endPosition) {
				UInt32 deltaTime = ParseDeltaTime(reader);
				// Console.WriteLine($"deltaTime: {deltaTime}");
				byte[] eventData = ParseEvent(reader);
				if (!Add(deltaTime, eventData)) {
					break;
				}
				if (isEnd) {
					break;
				}
			}
		}
		private UInt32 ParseDeltaTime(BinaryReader reader)
		{
			return ParseVariableLength(reader);
		}
		private UInt32 ParseVariableLength(BinaryReader reader)
		{
			UInt32 value = 0;
			byte b;
			do {
				b = reader.ReadByte();
				value = (value << 7) | (UInt32)(b & 0x7F);
			} while ((b & 0x80) != 0);
			return value;
		}
		private byte[] ParseEvent(BinaryReader reader)
		{
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
						smfPlayer.tempo = 60000000 / usecPerQuarterNote;
						smfPlayer.usecPerQuarterNote = (float)usecPerQuarterNote;
						// Console.WriteLine("Tempo: " + smfPlayer.tempo);
					} else if (metaType == 0x58) {
						// Time Signature
						smfPlayer.beat.count = metaData[2];
						smfPlayer.beat.unit = 2 ^ metaData[3];
						// Console.WriteLine($"Beat:  {smfPlayer.beat.count} / {smfPlayer.beat.unit}");
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
	};

	class TrackPlayer {
		public bool isEnd = false;
		private TrackData midiEvents;
		private SMFPlayer smfPlayer;
		private MIDIHandler midiHandler;
		private UInt32 currentTick = 0;
		private int currentEventIndex = 0;
		private UInt32 nextEventTick = 0;
		private UInt32 nextEventMsec = 0; 
		public TrackPlayer(TrackData data, SMFPlayer player, MIDIHandler handler) {
			midiEvents = data;
			smfPlayer = player;
			midiHandler = handler;
		}
		public void Reset()
		{
			midiEvents.Reset();
			isEnd = false;
			nextEventTick = midiEvents.GetDeltaTime();
			nextEventMsec = midiEvents.GetMsec();
			currentTick = 0;
		}
		public UInt32 Tickup()
		{
			if (isEnd) {
				return UInt32.MaxValue;
			}
			currentTick += 1;
			if (midiEvents.IsEnd()) {
				isEnd = true;
				return UInt32.MaxValue;
			}
			while (currentTick >= nextEventTick) {
				DoMIDIEvent(midiEvents.GetData());
				if (!midiEvents.Next()) {
					nextEventTick = UInt32.MaxValue;
					nextEventMsec = UInt32.MaxValue;
					isEnd = true;
					break;
				}
				nextEventTick = currentTick + midiEvents.GetDeltaTime();
				nextEventMsec = midiEvents.GetMsec();
				// Console.WriteLine($"currentTick: {currentTick}, nextEventTick:{nextEventTick}, nextEventMsec:{nextEventMsec}");
			}
			return nextEventMsec;
		}
		private string GetMetaText(byte[] data)
		{
			String text = System.Text.Encoding.UTF8.GetString(data, 2, data.Length - 2);
			return text;
		}
		private void DoMIDIEvent(byte[] data)
		{
			if (data[0] == 0xFF) {
				// Meta Event
				switch (data[1]) {
					case 0x51:
						// Set Tempo
						UInt32 usecPerQuarterNote = (UInt32)(data[2] << 16 | data[3] << 8 | data[4]);
						usecPerQuarterNote &= 0x00FFFFFF;
						smfPlayer.tempo = 60000000 / usecPerQuarterNote;
						smfPlayer.usecPerQuarterNote = usecPerQuarterNote;
						// Console.WriteLine("Tempo: " + smfPlayer.tempo);
						break;
					case 0x58:
						// Time Signature
						smfPlayer.beat.count = data[2];
						smfPlayer.beat.unit = 2 ^ data[3];
						// Console.WriteLine($"Beat:  {smfPlayer.beat.count} / {smfPlayer.beat.unit}");
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
	};
}
