using System.Threading.Tasks;
using Urho;
using Urho.Gui;
using Urho.Actions;
using Urho.Shapes;

namespace SamplyGame
{
	public class StartMenu : Component
	{
		TaskCompletionSource<bool> menuTaskSource;
		Node bigAircraft;
		Node rotor;
		Text textBlock;
		Node menuLight;
		bool finished = true;

		public StartMenu()
		{
			ReceiveSceneUpdates = true;
		}

		public async Task ShowStartMenu(bool gameOver)
		{
			var cache = Application.ResourceCache;
			bigAircraft = Node.CreateChild();
			var model = bigAircraft.CreateComponent<StaticModel>();

			if (gameOver)
			{
				model.Model = cache.GetModel(Assets.Models.Monkey);
				model.SetMaterial(cache.GetMaterial(Assets.Materials.Monkey).Clone(""));
				bigAircraft.SetScale(0.3f);
				bigAircraft.Rotate(new Quaternion(180, 90, 20), TransformSpace.Local);
			}
			else
			{
				model.Model = cache.GetModel(Assets.Models.Player1);
				model.SetMaterial(cache.GetMaterial(Assets.Materials.Player1D).Clone(""));
				bigAircraft.SetScale(1f);
				bigAircraft.Rotate(new Quaternion(0, 40, -50), TransformSpace.Local);
			}

			bigAircraft.Position = new Vector3(10, 2, 10);
			bigAircraft.RunActions(new RepeatForever(new Sequence(new RotateBy(1f, 0f, 0f, 5f), new RotateBy(1f, 0f, 0f, -5f))));

			//TODO: rotor should be defined in the model + animation
			rotor = bigAircraft.CreateChild();
			var rotorModel = rotor.CreateComponent<Box>();
			rotorModel.Color = Color.White;
			rotor.Scale = new Vector3(0.1f, 1.5f, 0.1f);
			rotor.Position = new Vector3(0, 0, -1.3f);
			var rotorAction = new RepeatForever(new RotateBy(1f, 0, 0, 360f*6)); //RPM
			rotor.RunActions(rotorAction);
			
			menuLight = bigAircraft.CreateChild();
			menuLight.Position = new Vector3(-3, 6, 2);
			menuLight.AddComponent(new Light { LightType = LightType.Point, Brightness = 0.3f });

			await bigAircraft.RunActionsAsync(new EaseIn(new MoveBy(1f, new Vector3(-10, -2, -10)), 2));

			textBlock = new Text();
			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Bottom;
			textBlock.Value = gameOver ? "GAME OVER" : "TAP TO START";
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
