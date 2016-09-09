using Windows.ApplicationModel.Core;
using Urho.HoloLens;

namespace HelloWorld
{
	// The entry point for the app.
	internal class AppViewSource : IFrameworkViewSource
	{
		public IFrameworkView CreateView()
		{
			return UrhoAppView.Create<HelloWorldApplication>(null); // null means only CoreData
		}
	}
}
