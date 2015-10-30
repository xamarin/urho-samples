using Urho;

namespace ShootySkies.Desktop
{
	class Program
	{
		static void Main(string[] args)
		{
			UrhoEngine.Init(@"../../Assets");
			new ShootySkiesGame(new Context()).Run();
		}
	}
}
