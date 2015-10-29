using System;

namespace ShootySkies
{
	public class RandomHelper
	{
		static readonly Random Random = new Random();

		/// <summary>
		/// Return a random float between 0.0 (inclusive) and 1.0 (exclusive.)
		/// </summary>
		public static float NextRandom() { return (float)Random.NextDouble(); }

		/// <summary>
		/// Return a random float between 0.0 and range, inclusive from both ends.
		/// </summary>
		public static float NextRandom(float range) { return (float)Random.NextDouble() * range; }

		/// <summary>
		/// Return a random float between min and max, inclusive from both ends.
		/// </summary>
		public static float NextRandom(float min, float max) { return (float)((Random.NextDouble() * (max - min)) + min); }

		/// <summary>
		/// Return a random integer between min and max - 1.
		/// </summary>
		public static int NextRandom(int min, int max) { return Random.Next(min, max); }
	}
}
