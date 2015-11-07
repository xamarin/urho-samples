using System;

namespace ShootySkies
{
	public class RandomHelper
	{
		static readonly Random Random = new Random();

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
