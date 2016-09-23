using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.Shapes;

namespace HelloWorld
{
	public class HelloWorldApplication : HoloApplication
	{
		Node earthNode;

		public HelloWorldApplication(string pak, bool emulator) : base(pak, emulator) { }

		protected override async void Start()
		{
			base.Start();

			earthNode = Scene.CreateChild();
			earthNode.Position = new Vector3(0, 0, 1);
			earthNode.SetScale(0.2f);

			var earth = earthNode.CreateComponent<Sphere>();
			earth.SetMaterial(Material.FromImage("Earth.jpg"));

			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.4f);
			moonNode.Position = new Vector3(1f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			moon.SetMaterial(Material.FromImage("Moon.jpg"));

			earthNode.RunActions(new RepeatForever(new RotateBy(1f, 0, 5, 0)));
			moonNode.RunActions( new RepeatForever(new RotateAroundBy(1f, earthNode.WorldPosition, 0, 10, 0)));
		}

		protected override void OnUpdate(float timeStep)
		{
			//For optical stabilization (optional)
			FocusWorldPoint = earthNode.WorldPosition;
		}
	}
}