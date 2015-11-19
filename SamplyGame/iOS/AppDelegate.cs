using Foundation;
using UIKit;
using Urho;
using Urho.iOS;

namespace SamplyGame.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			UrhoEngine.Init ();
			new SamplyGame (new Context ()).Run ();
			return true;
		}
	}
}