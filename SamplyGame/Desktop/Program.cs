using Urho;

namespace SamplyGame.Desktop
{
	class Program
	{
		static void Main(string[] args)
		{
			UrhoEngine.Init(@"../../Assets");
			new SamplyGame(new Context()).Run();
		}
	}
}
