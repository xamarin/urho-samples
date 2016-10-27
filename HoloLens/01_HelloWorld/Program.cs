using System;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.HoloLens;
using Urho.Shapes;
using Urho.Resources;

namespace HelloWorld
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<HelloWorldApplication>("Data"));
	}


	public class HelloWorldApplication : HoloApplication
	{
		Node earthNode;

		public HelloWorldApplication(ApplicationOptions opts) : base(opts) { }

		protected override void Start()
		{
			// base.Start() creates a basic scene
			base.Start();

			// Create a node for the Earth
			earthNode = Scene.CreateChild();
			earthNode.Position = new Vector3(0, 0, 1); // One meter away
			earthNode.SetScale(0.2f); // 20cm

			// Create a static model component (Sphere):
			var earth = earthNode.CreateComponent<StaticModel>();
			earth.Model = CoreAssets.Models.Sphere; // Built-in assets can be accessed via CoreAssets
			// Apply a material (material is a set of tecniques, parameters and textures)
			earth.SetMaterial(ResourceCache.GetMaterial("Materials/Earth.xml"));

			// Same for the Moon
			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.27f); // Relative size of the Moon is 1738.1km/6378.1km
			moonNode.Position = new Vector3(1.2f, 0, 0);

			// Instead of Sphere, let's use Urho.Shapes
			// Sphere component is basically the same StaticModel with CoreData/Models/Sphere.mdl model
			var moon = moonNode.CreateComponent<Sphere>();
			// Material.FromImage is the easiest way to create a material from an image (using simple CoreAssets.Techniques.Diff technique)
			moon.SetMaterial(Material.FromImage("Textures/Moon.jpg"));

			// Run a an action to spin the Earth
			earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -5, deltaAngleZ: 0)));
		}

		// For HL optical stabilization (optional)
		public override Vector3 FocusWorldPoint => earthNode.WorldPosition;
	}
}