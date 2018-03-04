using Foundation;
using System.Threading.Tasks;
using UIKit;
using ARKit;
using System.Linq;
using Urho;
using AVFoundation;
using MonoTouch.Dialog;

namespace ARKitXamarinDemo
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			var rootElement = new RootElement("UrhoSharp");
			Window.RootViewController = new DialogViewController(rootElement);
			var section = new Section("ARKit samples", "UrhoSharp");
			rootElement.Add(section);
			section.Add(new StringElement("Mutant demo", () => Run<MutantDemo>()));
			section.Add(new StringElement("Vertical surfaces", () => Run<VerticalSurfaces>()));
			section.Add(new StringElement("Crowd demo", () => Run<CrowdDemo>()));
			section.Add(new StringElement("iPhone X face demo", () => Run<FaceDemo>()));

			Window.MakeKeyAndVisible();
			return true;
		}

        static void Run<T>() where T : Urho.Application
		{
			var app = Urho.Application.CreateInstance<T>(new ApplicationOptions 
			{
				ResourcePaths = new[] { "UrhoData" }, 
				DelayedStart = false, // if TRUE then Engine.RunFrame() will be executed each ARKit update, otherwise - a separate game loop
				Orientation = ApplicationOptions.OrientationType.Portrait
			});
			app.Run();
		}
	}
}

