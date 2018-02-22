using System;
using System.Diagnostics;
using Urho;
using ARKit;
using Urho.iOS;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace UrhoAR
{
	public partial class MyApp : SimpleApplication
	{
		ARKitComponent arkitComponent;

		void SetupAR()
		{
			arkitComponent = Scene.CreateComponent<ARKitComponent>();
			arkitComponent.Camera = Camera;
			arkitComponent.Orientation = UIKit.UIInterfaceOrientation.Portrait;
			arkitComponent.ARConfiguration = new ARWorldTrackingConfiguration {
				PlaneDetection = ARPlaneDetection.Horizontal,
			};
			arkitComponent.Run(Options.DelayedStart);
		}
	}
}