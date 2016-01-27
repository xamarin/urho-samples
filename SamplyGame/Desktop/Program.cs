using Urho;
using Urho.Desktop;

namespace SamplyGame.Desktop
{
	class Program
	{
		static void Main(string[] args)
		{
			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
			new SamplyGame().Run();
		}
	}
}
