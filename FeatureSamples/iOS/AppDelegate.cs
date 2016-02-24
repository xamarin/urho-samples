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
		public override UIWindow Window { get;set;}
		
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			Window = new UIWindow (UIScreen.MainScreen.Bounds);
			Window.RootViewController = new DialogViewController (new RootElement ("UrhoSharp") {
				new Section ("Feature Samples"){
					from type in typeof (HelloWorld).Assembly.GetTypes ()
						where type.IsSubclassOf (typeof (Sample))
					select new StringElement (type.Name, () => Run (type))
				}
			});
			Window.MakeKeyAndVisible ();
			return true;
		}

		static void Run<T> ()
 		{
			Run (typeof(T));
 		}

		static void Run(System.Type type)
		{
			Urho.Application.CreateInstance(type, new ApplicationOptions("Data")).Run();
		}
	}
}