using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class StartMenu : Component
	{
		TaskCompletionSource<bool> menuTaskSource;
		Node bigAircraft;
		Node rotor;
		Text textBlock;
		Node menuLight;

		public StartMenu(Context context) : base(context) {}

		public async Task ShowStartMenu()
		{
			var cache = Application.ResourceCache;
			bigAircraft = Node.CreateChild();
			var model = bigAircraft.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Player.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/Player.xml").Clone(""));
			bigAircraft.SetScale(1.2f);
			bigAircraft.Rotate(new Quaternion(0, 220, 40), TransformSpace.Local);
			bigAircraft.Position = new Vector3(10, 2, 10);

			//TODO: rotor should be defined in the model + animation
			rotor = bigAircraft.CreateChild();
			var rotorModel = rotor.CreateComponent<StaticModel>();
			rotorModel.Model = cache.GetModel("Models/Box.mdl");
			rotorModel.SetMaterial(cache.GetMaterial("Materials/Black.xml"));
			rotor.Scale = new Vector3(0.1f, 1.6f, 0.1f);
			rotor.Rotation = new Quaternion(0, 0, 0);
			rotor.Position = new Vector3(0, -0.15f, 1);
			rotor.RunActionsAsync(new RepeatForever(new RotateBy(1f, 0, 0, 360f * 3))); //RPM

			menuLight?.Remove();
			menuLight = bigAircraft.CreateChild();
			menuLight.Position = new Vector3(-3, 6, 2);
			menuLight.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 14, Brightness = 1f });

			await bigAircraft.RunActionsAsync(new EaseIn(new MoveBy(1f, new Vector3(-10, -2, -10)), 2));

			textBlock = new Text(Context);
			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Bottom;
			textBlock.Value = "TAP TO START";
			textBlock.SetFont(cache.GetFont("Fonts/BlueHighway.ttf"), 28);
			Application.UI.Root.AddChild(textBlock);

			menuTaskSource = new TaskCompletionSource<bool>();
			Application.SceneUpdate += OnSceneUpdate;

			await menuTaskSource.Task;
		}

		private async void OnSceneUpdate(SceneUpdateEventArgs args)
		{
			var input = Application.Input;
			if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches > 0)
			{
				Application.UI.Root.RemoveChild(textBlock, 0);
				await bigAircraft.RunActionsAsync(new EaseIn(new MoveBy(1f, new Vector3(-10, -2, -10)), 3));
				rotor.RemoveAllActions();
				//TODO: remove scene
				menuTaskSource.TrySetResult(true);
				Application.SceneUpdate -= OnSceneUpdate;
			}
		}
	}
}
