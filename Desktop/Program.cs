using System;
using System.Linq;

namespace Urho.Samples.Desktop
{
	class Program
	{
		static System.Type[] samples;

		static void Main(string[] args)
		{
			//args =  new [] { "41" };

			FindAvailableSamplesAndPrint();
			System.Type selectedSampleType = null;

			if (args.Length > 0)
				selectedSampleType = ParseSampleFromNumber(args[0]);

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				while (selectedSampleType == null)
				{
					Console.WriteLine("Enter a sample number [1-40]:");
					selectedSampleType = ParseSampleFromNumber(Console.ReadLine());
				}
			}
			else if (selectedSampleType == null)
			{
				selectedSampleType = typeof(_23_Water); //show 23_Water sample by default for OS X if args are empty.
			}

			var resourcesDirectory = @"../../Assets";
			//special assets for AtomicEngine based samples:
			if (selectedSampleType == typeof (_41_ToonTown))
			{
				resourcesDirectory = @"../../Assets/AtomicEngineAssets";
			}

			var code = ApplicationLauncher.Run(() => (Application)Activator.CreateInstance(selectedSampleType, new Context()), resourcesDirectory);
			Console.WriteLine($"Exit code: {code}. Press any key to exit...");
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
				Console.WriteLine("Invalid format.");
				return null;
			}

			var sample = samples.FirstOrDefault(s => s.Name.StartsWith($"_{number.ToString("00")}"));
			if (sample == null)
			{
				Console.WriteLine("Sample was not found");
				return null;
			}

			return sample;
		}

		static void FindAvailableSamplesAndPrint()
		{
			samples = typeof(Sample).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample)).ToArray();
			foreach (var sample in samples)
				Console.WriteLine(sample.Name);
			Console.WriteLine();
		}
	}
}
