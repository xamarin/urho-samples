using System;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.HoloLens;
using Urho.Shapes;

namespace HelloWorld
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
			public IFrameworkView CreateView() => UrhoAppView.Create<HelloWorldApplication>("Data");
		}
	}


	public class HelloWorldApplication : HoloApplication
	{
		Node earthNode;

		public HelloWorldApplication(string pak, bool emulator) : base(pak, emulator) { }

		protected override async void Start()
		{
			// base.Start() creates a basic scene, see
			// https://github.com/xamarin/urho/blob/master/Bindings/Portable/Holographics/HoloApplication.cs#L98-L155
			base.Start();

			// Create a node for the Earth
			earthNode = Scene.CreateChild();
			earthNode.Position = new Vector3(0, 0, 1); //one meter away
			earthNode.SetScale(0.2f); //20cm
			earthNode.Rotation = new Quaternion(x: 0, y: 23.26f, z: 0); // Earth's obliquity is 23°26′

			// Create a Sphere component which is basically 
			// a StaticModel with CoreData\Models\Sphere.mdl model
			var earth = earthNode.CreateComponent<Sphere>();
			// Apply a material (material is a set of tecniques, parameters and textures)
			earth.SetMaterial(ResourceCache.GetMaterial("Earth.xml"));

			// Let's create a second sphere (slightly bigger - 1.01f) for clouds
			var cloudsNode = earthNode.CreateChild();
			cloudsNode.SetScale(1.01f);
			// Alternative way to create a sphere - create a StaticModel and set Model to Sphere.mdl
			var clouds = cloudsNode.CreateComponent<StaticModel>();
			clouds.Model = CoreAssets.Models.Sphere; //same as ResourceCache.GetModel("Models/Sphere.mdl");

			// Create a material _in code_ for the clouds
			var cloudsMaterial = new Material();
			cloudsMaterial.SetTechnique(0, CoreAssets.Techniques.DiffAdd, 1, 1);
			cloudsMaterial.SetTexture(TextureUnit.Diffuse, ResourceCache.GetTexture2D("Earth_Clouds.jpg"));
			clouds.SetMaterial(cloudsMaterial);

			// By default HoloApplication has a bult-in light (directional)
			// decrease the brightness
			DirectionalLight.Brightness -= 0.6f;

			// Same for the Moon
			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.4f);
			moonNode.Position = new Vector3(1f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			// Material.FromImage is the easiest way to create a material from an image (using Diff.xml technique)
			moon.SetMaterial(Material.FromImage("Moon.jpg"));

			// Run a few actions to spin the Earth, the Moon and the clouds.
			earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -4, deltaAngleZ: 0)));
			cloudsNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: 1, deltaAngleZ: 0)));
			moonNode.RunActions(new RepeatForever(new RotateAroundBy(1f, earthNode.WorldPosition, 0, -3, 0)));
		}

		protected override void OnUpdate(float timeStep)
		{
			//For HL optical stabilization (optional)
			FocusWorldPoint = earthNode.WorldPosition;
		}
	}
}