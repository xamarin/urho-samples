using System;
using System.Linq;
using Urho.Desktop;

namespace Urho.Samples.Mac
{
	class Program
	{
		static Type[] samples;

		/// <param name="args">sample number, e.g. "19"</param>
		static void Main(string[] args)
		{
			//   UrhoEngine.Init(pathToAssets);
			//   new Water().Run();
			//   return;

			samples = typeof(Sample).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample)).ToArray();
			Type selectedSampleType = args.Length > 0 ? ParseSampleFromNumber(args[0]) : typeof (BasicTechniques);

			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
			var game = (Application) Activator.CreateInstance(selectedSampleType, new ApplicationOptions("Data"));
			var exitCode = game.Run();
			Console.WriteLine($"Exit code: {exitCode}. Press any key to exit...");
			Console.ReadKey();
		}

		static Type ParseSampleFromNumber(string input)
		{
			int number;
			if (!int.TryParse(input, out number))
			{
				Console.WriteLine("Invalid format.");
				return null;
			}

			if (number >= samples.Length || number < 0)
			{
				Console.WriteLine("Invalid number.");
				return null;
			}

			return samples[number];
		}
	}
}
