using System;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;

namespace HelloWorld
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(
			new UrhoAppViewSource<HelloWorldApplication>(
				new ApplicationOptions("Data")));
	}

	public class HelloWorldApplication : StereoApplication
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

			// Create a static model component - Sphere:
			var earth = earthNode.CreateComponent<Sphere>();
			// Materials are usually more complicated than just textures, but for such
			// simple cases we can use quick FromImage method to create a material from an image.
			earth.SetMaterial(Material.FromImage("Textures/Earth.jpg"));

			// Same steps for the Moon
			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.27f); // Relative size of the Moon is 1738.1km/6378.1km
			moonNode.Position = new Vector3(1.2f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			moon.SetMaterial(Material.FromImage("Textures/Moon.jpg"));

			// Run a an action to spin the Earth (5 degrees per second)
			earthNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -5, deltaAngleZ: 0)));
		}

		// For HL optical stabilization (optional)
		public override Vector3 FocusWorldPoint => earthNode.WorldPosition;
	}
}