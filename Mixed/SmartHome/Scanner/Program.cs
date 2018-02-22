using System;
using Windows.ApplicationModel.Core;
using Urho;

namespace SmartHome.HoloLens
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
		private static void Main() => CoreApplication.Run(new UrhoAppViewSource<ScannerApp>());
	}
}