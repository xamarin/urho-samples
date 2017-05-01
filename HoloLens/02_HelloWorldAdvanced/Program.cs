using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;

namespace _07_HelloWorldWithCustomShaders
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
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<HelloWorldApplication>(new ApplicationOptions("Data")));
	}

	public class HelloWorldApplication : StereoApplication
	{
		Node earthNode;
		Vector3 earthPosBeforManipulations;
		Material earthMaterial;
		float cloudsOffset;

		public HelloWorldApplication(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();

			EnableGestureManipulation = true;
			EnableGestureTapped = true;

			Log.LogLevel = LogLevel.Warning;
			Log.LogMessage += l => { Debug.WriteLine(l.Level + ":  " + l.Message); };

			// Create a node for the Earth
			earthNode = Scene.CreateChild();
			earthNode.Position = new Vector3(0, 0, 1.5f);
			earthNode.SetScale(0.3f);

			DirectionalLight.Brightness = 1f;
			DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

			var earth = earthNode.CreateComponent<Sphere>();
			earthMaterial = ResourceCache.GetMaterial("Materials/Earth.xml");
			earth.SetMaterial(earthMaterial);

			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.27f);
			moonNode.Position = new Vector3(1.2f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			moon.SetMaterial(ResourceCache.GetMaterial("Materials/Moon.xml"));

			// HolographicDisplay api is available in >=10.0.15063
			// var display = Windows.Graphics.Holographic.HolographicDisplay.GetDefault();
			// bool isHoloLens = display != null && !display.IsOpaque;
			bool isHoloLens = true;

			if (!isHoloLens)
			{
				//Since the display is opaque - we can display a skybox with stars
				var skyboxNode = Scene.CreateChild();
				skyboxNode.SetScale(100);
				var skybox = skyboxNode.CreateComponent<Skybox>();
				skybox.Model = CoreAssets.Models.Box;
				//Skybox is usally a 6 textures joined together, see FeatureSamples/Core/23_Water sample
				skybox.SetMaterial(Material.SkyboxFromImage("Textures/Space.png"));
			}

			// Run a few actions to spin the Earth, the Moon and the clouds.
			earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));
		}

		protected override void OnUpdate(float timeStep)
		{
			// Move clouds via CloudsOffset (defined in the material.xml and used in the PS)
			cloudsOffset += 0.00005f;
			earthMaterial.SetShaderParameter("CloudsOffset", new Vector2(cloudsOffset, 0));
			//NOTE: this could be done via SetShaderParameterAnimation
		}

		// For HL optical stabilization (optional)
		public override Vector3 FocusWorldPoint => earthNode.WorldPosition;

		public override void OnGestureManipulationStarted()
		{
			earthPosBeforManipulations = earthNode.Position;
		}

		public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition)
		{
			earthNode.Position = relativeHandPosition + earthPosBeforManipulations;
		}

		public override void OnGestureDoubleTapped()
		{
			earthNode.Scale *= 1.2f;
		}
	}
}