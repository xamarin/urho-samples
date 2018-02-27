using System;
using System.Diagnostics;
using Urho;
using Urho.Droid;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;
using Com.Google.AR.Core;

namespace UrhoAR
{
	public partial class MyApp : SimpleApplication
	{
		ARCoreComponent ArCore;

		void SetupAR()
		{
			ArCore = Scene.CreateComponent<ARCoreComponent>();
			ArCore.ARFrameUpdated += OnARFrameUpdated;
			ArCore.ConfigRequested += ArCore_ConfigRequested;
			ArCore.Run();
		}

		void ArCore_ConfigRequested(Config config)
		{
			config.SetPlaneFindingMode(Config.PlaneFindingMode.Horizontal);
			config.SetLightEstimationMode(Config.LightEstimationMode.AmbientIntensity);
			config.SetUpdateMode(Config.UpdateMode.LatestCameraImage); //non blocking
		}

		void OnARFrameUpdated(Frame arFrame)
		{
		}
	}
}