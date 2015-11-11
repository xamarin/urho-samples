using Foundation;
using UIKit;
using Urho;
using Urho.iOS;

namespace ShootySkies.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			UrhoEngine.Init ();
			new ShootySkiesGame (new Context ()).Run ();
			return true;
		}
	}
}