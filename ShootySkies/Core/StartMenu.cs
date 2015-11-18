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
		bool finished = true;

		public StartMenu(Context context) : base(context)
		{
			ReceiveSceneUpdates = true;
		}

		public async Task ShowStartMenu()
		{
			var cache = Application.ResourceCache;
			bigAircraft = Node.CreateChild();
			var model = bigAircraft.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Player);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Player).Clone(""));
			bigAircraft.SetScale(1.2f);
			bigAircraft.Rotate(new Quaternion(0, 220, 40), TransformSpace.Local);
			bigAircraft.Position = new Vector3(10, 2, 10);

			//TODO: rotor should be defined in the model + animation
			rotor = bigAircraft.CreateChild();
			var rotorModel = rotor.CreateComponent<StaticModel>();
			rotorModel.Model = cache.GetModel(Assets.Models.Box);
			rotorModel.SetMaterial(cache.GetMaterial(Assets.Materials.Black));
			rotor.Scale = new Vector3(0.1f, 1.6f, 0.1f);
			rotor.Rotation = new Quaternion(0, 0, 0);
			rotor.Position = new Vector3(0, -0.15f, 1);
			rotor.RunActionsAsync(new RepeatForever(new RotateBy(1f, 0, 0, 360f * 3))); //RPM

			menuLight = bigAircraft.CreateChild();
			menuLight.Position = new Vector3(-3, 6, 2);
			menuLight.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 14, Brightness = 1f });

			await bigAircraft.RunActionsAsync(new EaseIn(new MoveBy(1f, new Vector3(-10, -2, -10)), 2));

			textBlock = new Text(Context);
			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Bottom;
			textBlock.Value = "TAP TO START";
			textBlock.SetFont(cache.GetFont(Assets.Fonts.Font), Application.Graphics.Width / 15);
			Application.UI.Root.AddChild(textBlock);

			menuTaskSource = new TaskCompletionSource<bool>();
			finished = false;
			await menuTaskSource.Task;
		}

		protected override async void OnUpdate(float timeStep)
		{
			if (finished)
				return;

			var input = Application.Input;
			if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches > 0)
			{
				finished = true;
				Application.UI.Root.RemoveChild(textBlock, 0);
				await bigAircraft.RunActionsAsync(new EaseIn(new MoveBy(1f, new Vector3(-10, -2, -10)), 3));
				rotor.RemoveAllActions();
				menuTaskSource.TrySetResult(true);
			}
		}
	}
}
