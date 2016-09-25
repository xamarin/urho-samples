using System;
using Windows.ApplicationModel.Core;
using Urho.HoloLens;

namespace Physics
{
	/// <summary>
	/// Windows Holographic application using UrhoSharp.
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

		class AppViewSource : IFrameworkViewSource
		{
			public IFrameworkView CreateView() => UrhoAppView.Create<PhysicsSample>("Data");
		}
	}

}