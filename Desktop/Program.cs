using System;
using System.Linq;

namespace Urho.Samples.Desktop
{
	class Program
	{
		static System.Type[] samples;

		static void Main(string[] args)
		{
			//args =  new [] { "39" };

			FindAvailableSamplesAndPrint();
			System.Type selectedSampleType = null;

			if (args.Length > 0)
			{
				// try to get a desired sample's number to run via command line args:
				selectedSampleType = ParseSampleFromNumber(args[0]);
			}

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				while (selectedSampleType == null)
				{
					WriteLine("Enter a sample number [1-41]:", ConsoleColor.White);
					selectedSampleType = ParseSampleFromNumber(Console.ReadLine());
				}
			}
			else if (selectedSampleType == null)
			{
				selectedSampleType = typeof(_41_ToonTown); //show _41_ToonTown sample by default for OS X if args are empty.
			}

			var resourcesDirectory = @"../../Assets";
			//special assets for AtomicEngine based samples:
			if (selectedSampleType == typeof (_41_ToonTown))
			{
				resourcesDirectory = @"../../Assets/AtomicEngineAssets";
			}

			var code = ApplicationLauncher.Run(() => (Application)Activator.CreateInstance(selectedSampleType, new Context()), resourcesDirectory);
			WriteLine($"Exit code: {code}. Press any key to exit...", ConsoleColor.DarkYellow);
			Console.ReadKey();
		}

		/// <summary>
		/// Finds sample by number, e.g.:
		/// 2 -> _02_HelloGui
		/// </summary>
		static System.Type ParseSampleFromNumber(string input)
		{
			int number;
			if (!int.TryParse(input, out number))
			{
				WriteLine("Invalid format.", ConsoleColor.Red);
				return null;
			}

			var sample = samples.FirstOrDefault(s => s.Name.StartsWith($"_{number.ToString("00")}"));
			if (sample == null)
			{
				WriteLine("Sample was not found", ConsoleColor.Red);
				return null;
			}

			return sample;
		}

		static void FindAvailableSamplesAndPrint()
		{
			var highlightedSamples = new [] { typeof(_41_ToonTown) };
			samples = typeof(Sample).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample)).ToArray();
			foreach (var sample in samples)
			{
				WriteLine(sample.Name, highlightedSamples.Contains(sample) ? ConsoleColor.Yellow : ConsoleColor.DarkGray);
			}
			Console.WriteLine();
		}

		static void WriteLine(string text, ConsoleColor consoleColor)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = consoleColor;
			Console.WriteLine(text);
			Console.ForegroundColor = color;
		}
	}
}
