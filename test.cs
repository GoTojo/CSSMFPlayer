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
			Console.WriteLine("Hello, World!");
			string smfPath = @"らーめん食べよう.mid";
			Visualizer visualizer = new Visualizer();
			MidiWatcher midiWatcher = new MidiWatcher(visualizer);
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
	}
};
