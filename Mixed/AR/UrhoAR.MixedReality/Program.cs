using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;

namespace UrhoAR.MixedReality
{
	internal class Program
	{
		[MTAThread]
		static void Main()
		{
			var appViewSource = new UrhoAppViewSource<MyApp>(new ApplicationOptions("MyData"));
			appViewSource.UrhoAppViewCreated += OnViewCreated;
			CoreApplication.Run(appViewSource);
		}

		static void OnViewCreated(UrhoAppView view)
		{
			view.WindowIsSet += View_WindowIsSet;
		}

		static void View_WindowIsSet(Windows.UI.Core.CoreWindow coreWindow)
		{
			// you can subscribe to CoreWindow events here
		}
	}
}