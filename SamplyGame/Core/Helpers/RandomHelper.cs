using System;

namespace SamplyGame
{
	public class RandomHelper
	{
		static readonly Random Random = new Random();

		/// <summary>
		/// Return a random float between min and max, inclusive from both ends.
		/// </summary>
		public static float NextRandom(float min, float max) => (float)((Random.NextDouble() * (max - min)) + min);

		/// <summary>
		/// Return a random integer between min and max - 1.
		/// </summary>
		public static int NextRandom(int min, int max) => Random.Next(min, max);

		/// <summary>
		/// Return a random boolean
		/// </summary>
		public static bool NextBoolRandom() => Random.Next(0, 2) == 1;
	}
}
