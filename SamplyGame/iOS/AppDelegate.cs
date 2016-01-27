using Foundation;
using UIKit;
using Urho;
using Urho.iOS;
using System.Threading.Tasks;

namespace SamplyGame.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			LaunchGame();
			return true;
		}

		async void LaunchGame()
		{
			await Task.Yield();
			new SamplyGame().Run();
		}
	}
}