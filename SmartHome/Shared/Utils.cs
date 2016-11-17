using System;

namespace Shared
{
	public static class Utils
	{
		public static bool TryParseIpAddress(string str, out string ip, out int port)
		{
			ip = null;
			port = 0;

			var parts = str.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
			if (parts != null && parts.Length == 2)
			{
				ip = parts[0];
				if (int.TryParse(parts[1], out port))
				{
					return true;
				}
			}
			return false;
		}
	}
}
