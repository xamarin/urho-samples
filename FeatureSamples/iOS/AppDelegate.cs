using System.Linq;
using Foundation;
using UIKit;
using Urho.iOS;
using MonoTouch.Dialog;

namespace Urho.Samples.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			var window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.RootViewController = new DialogViewController (new RootElement ("UrhoSharp") {
				new Section ("Feature Samples"){
					from type in typeof (HelloWorld).Assembly.GetTypes ()
						where type.IsSubclassOf (typeof (Sample))
					select new StringElement (type.Name, () => Run (type))
				}
			});
			window.MakeKeyAndVisible ();
			UrhoEngine.Init();
			return true;
		}

		static void Run<T> ()
 		{
			Run (typeof(T));
 		}

		static void Run(System.Type type)
		{
			Urho.Application.CreateInstance(type).Run();
		}
	}
}