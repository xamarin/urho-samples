using Windows.ApplicationModel.Core;
using Urho.HoloLens;

namespace Mutant
{
	// The entry point for the app.
	internal class AppViewSource : IFrameworkViewSource
	{
		public IFrameworkView CreateView()
		{
			return UrhoAppView.Create<MutantApp>("MutantData"); // null means only CoreData
		}
	}
}
