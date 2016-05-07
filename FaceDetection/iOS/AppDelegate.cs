using System;
using Foundation;
using UIKit;
using Urho;
using Urho.iOS;
using System.Threading.Tasks;

namespace FaceDetection.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			LaunchGame();
			return true;
		}

		async void LaunchGame()
		{
			await Task.Yield();
			throw new NotImplementedException();
			//new UrhoApp(new ApplicationOptions("Data")).Run();
		}
	}
}


