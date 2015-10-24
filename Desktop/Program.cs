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
					WriteLine("Enter a sample number:", ConsoleColor.White);
					selectedSampleType = ParseSampleFromNumber(Console.ReadLine());
				}
			}
			else if (selectedSampleType == null)
			{
				selectedSampleType = typeof(ToonTown); //show ToonTown sample by default for OS X if args are empty.
			}

			var resourcesDirectory = @"../../Assets";
			//special assets for AtomicEngine based samples:
			if (selectedSampleType == typeof (ToonTown))
			{
				resourcesDirectory = @"../../Assets/AtomicEngineAssets";
			}

			UrhoEngine.Init(resourcesDirectory);
			var game = (Application) Activator.CreateInstance(selectedSampleType, new Context());
			var exitCode = game.Run();
			WriteLine($"Exit code: {exitCode}. Press any key to exit...", ConsoleColor.DarkYellow);
			Console.ReadKey();
		}

		static Type ParseSampleFromNumber(string input)
		{
			int number;
			if (!int.TryParse(input, out number))
			{
				WriteLine("Invalid format.", ConsoleColor.Red);
				return null;
			}

			if (number > samples.Length || number < 0)
			{
				WriteLine("Invalid number.", ConsoleColor.Red);
				return null;
			}

			return samples[number - 1];
		}

		static void FindAvailableSamplesAndPrint()
		{
			var highlightedSamples = new [] { typeof(ToonTown), typeof(Skies) };
			samples = typeof(Sample).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample)).ToArray();
			for (int index = 1; index <= samples.Length; index++)
			{
				var sample = samples[index - 1];
				WriteLine($"{index}. {sample.Name}", highlightedSamples.Contains(sample) ? ConsoleColor.Yellow : ConsoleColor.DarkGray);
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
