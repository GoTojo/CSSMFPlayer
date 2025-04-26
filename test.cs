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
			smfPlayer.Play();
			while (smfPlayer.isPlaying()) {
				Task.Delay(100);
			}
		}
	}
};
