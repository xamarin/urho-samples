using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.Shapes;

namespace HelloWorld
{
	public class HelloWorldApplication : HoloApplication
	{
		Node boxNode;

		public HelloWorldApplication(string pak) : base(pak) { }

		protected override async void Start()
		{
			base.Start();

			EnableGestureTapped = true; // Receive Tapped event (click)

			boxNode = Scene.CreateChild();
			boxNode.Rotation = new Quaternion(0, 45, 0);
			SetBoxPosition(new Vector3(0, 0, 2)); //1 meter
			var boxModelNode = boxNode.CreateChild();
			boxModelNode.SetScale(0.3f); //30cm*30cm*30cm
			var box = boxModelNode.CreateComponent<Box>();
			box.Color = Color.Green;

			await boxModelNode.RunActionsAsync(new TintTo(3f, 1, 1, 1));
			boxNode.RunActions(new RepeatForever(new RotateBy(1f, 0, 90, 0)));
		}

		public override void OnGestureTapped(GazeInfo gaze)
		{
			SetBoxPosition(gaze.Position + (2f /*Z meters*/ * gaze.Forward));
			base.OnGestureTapped(gaze);
		}

		void SetBoxPosition(Vector3 pos)
		{
			boxNode.Position = pos;

			//for optical stabilization:
			FocusWorldPoint = boxNode.WorldPosition;
		}
	}
}