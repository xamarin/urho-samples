using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.HoloLens;
using Urho.Shapes;

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
		private static void Main()
		{
			CoreApplication.Run(new AppViewSource());
		}

		class AppViewSource : IFrameworkViewSource
		{
			public IFrameworkView CreateView() => UrhoAppView.Create<HelloWorldApplication>("Data");
		}
	}

	public class HelloWorldApplication : HoloApplication
	{
		Node earthNode;
		Vector3 earthPosBeforManipulations;
		Material earthMaterial;
		float cloudsOffset;

		public HelloWorldApplication(string assets) : base(assets) { }

		protected override async void Start()
		{
			base.Start();

			EnableGestureManipulation = true;

			Log.LogLevel = LogLevel.Warning;
			Log.LogMessage += l => { Debug.WriteLine(l.Level + ":  " + l.Message); };

			// Create a node for the Earth
			earthNode = Scene.CreateChild();
			earthNode.Position = new Vector3(0, 0, 1); //one meter away
			earthNode.SetScale(0.2f); //20cm
			earthNode.Rotation = new Quaternion(x: 0, y: 23.26f, z: 0); // Earth's obliquity is 23°26′

			DirectionalLight.Brightness = 0.5f;
			DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0f));

			var earth = earthNode.CreateComponent<Sphere>();
			earthMaterial = ResourceCache.GetMaterial("Materials/Earth.xml");
			earth.SetMaterial(earthMaterial);

			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.27f);
			moonNode.Position = new Vector3(1.2f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			moon.SetMaterial(ResourceCache.GetMaterial("Materials/Moon.xml"));

			// Run a few actions to spin the Earth, the Moon and the clouds.
			earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));
		}

		protected override void OnUpdate(float timeStep)
		{
			// Move clouds via CloudsOffset (defined in the material.xml and used in the PS)
			cloudsOffset += 0.00005f;
			earthMaterial.SetShaderParameter("CloudsOffset", new Vector2(cloudsOffset, cloudsOffset / 2));
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
	}
}