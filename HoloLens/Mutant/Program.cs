using System;
using Windows.ApplicationModel.Core;
using Urho.HoloLens;

namespace Mutant
{
    /// <summary>
    /// Windows Holographic application using SharpDX.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [MTAThread]
        private static void Main()
        {
            var exclusiveViewApplicationSource = new AppViewSource();
            CoreApplication.Run(exclusiveViewApplicationSource);
        }
	}

	// The entry point for the app.
	internal class AppViewSource : IFrameworkViewSource
	{
		public IFrameworkView CreateView()
		{
			return UrhoAppView.Create<MutantApp>("Data"); // Assets packed into a MutantData.pak by PackageTool, see https://urho3d.github.io/documentation/1.6/_tools.html
		}
	}
}