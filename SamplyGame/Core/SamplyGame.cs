using System.Threading.Tasks;
using SamplyGame.Aircrafts.Enemies;
using Urho;
using Urho.Gui;
using Urho.Physics;
using Urho.Actions;

namespace SamplyGame
{
	public class SamplyGame : Application
	{
		const string CoinstFormat = "{0} coins";

		int coins;
		Scene scene;
		Text coinsText;

		public Player Player { get; private set; }

		public Viewport Viewport { get; private set; }

		public SamplyGame() : base(new ApplicationOptions(assetsFolder: "Data") { Height = 736, Width = 414, Orientation = ApplicationOptions.OrientationType.Portrait}) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			Input.SubscribeToKeyDown(e => { if (e.Key == Key.Esc) Engine.Exit(); });
		}

		async void CreateScene()
		{
			scene = new Scene();
			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));

			// Camera
			var cameraNode = scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
			cameraNode.CreateComponent<Camera>();
			
			Renderer.SetViewport(0, Viewport = new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null));

			// UI
			coinsText = new Text();
			coinsText.HorizontalAlignment = HorizontalAlignment.Right;
			coinsText.SetFont(ResourceCache.GetFont(Assets.Fonts.Font), Graphics.Width / 20);
			UI.Root.AddChild(coinsText);
			Input.SetMouseVisible(true, false);

			// Background
			var background = new Background();
			scene.AddComponent(background);
			background.Start();

			// Lights:
			var lightNode1 = scene.CreateChild();
			lightNode1.Position = new Vector3(0, -5, -40);
			lightNode1.AddComponent(new Light {  Range = 120, Brightness = 1.5f });

			var lightNode2 = scene.CreateChild();
			lightNode2.Position = new Vector3(10, 15, -12);
			lightNode2.AddComponent(new Light {  Range = 30.0f, Brightness = 1.5f });

			// Game logic cycle
			while (true)
			{
				var startMenu = scene.CreateComponent<StartMenu>();
				await startMenu.ShowStartMenu(); //wait for "start"
				startMenu.Remove();
				await StartGame();
			}
		}

		async Task StartGame()
		{
			UpdateCoins(0);
			Player = new Player();
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(Player);
			var playersLife = Player.Play();
			Enemies enemies = new Enemies(Player);
			scene.AddComponent(enemies);
			SpawnCoins();
			enemies.StartSpawning();
			await playersLife;
			enemies.KillAll();
			aircraftNode.Remove();
		}
		
		async void SpawnCoins()
		{
			var player = Player;
			while (Player.IsAlive && player == Player)
			{
				var coinNode = scene.CreateChild();
				coinNode.Position = new Vector3(RandomHelper.NextRandom(-2.5f, 2.5f), 5f, 0);
				var coin = new Coin();
				coinNode.AddComponent(coin);
				await coin.FireAsync(false);
				await scene.RunActionsAsync(new DelayTime(3f));
				coinNode.Remove();
			}
		}

		public void OnCoinCollected() => UpdateCoins(coins + 1);

		void UpdateCoins(int amount)
		{
			if (amount % 5 == 0 && amount > 0)
			{
				// give player a MassMachineGun each time he earns 5 coins
				Player.Node.AddComponent(new MassMachineGun());
			}
			coins = amount;
			coinsText.Value = string.Format(CoinstFormat, coins);
		}
	}
}
