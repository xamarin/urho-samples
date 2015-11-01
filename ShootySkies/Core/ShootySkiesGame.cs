using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class ShootySkiesGame : Application
	{
		const string CoinstFormat = "{0} coins";
		const int EnemySpawningIntensivity = 4;

		int coins;
		Scene scene;
		Player player;
		Text coinsText;
		List<Enemy> enemies;

		public Viewport Viewport { get; private set; }

		public ShootySkiesGame(Context c) : base(c, new ApplicationOptions { Height = 800, Width = 500, Orientation = ApplicationOptions.OrientationType.Portrait }) { }

		public override void Start()
		{
			base.Start();
			CreateScene();
			SubscribeToKeyDown(e => { if (e.Key == Key.Esc) Engine.Exit(); });
		}

		async void CreateScene()
		{
			scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));

			// Camera
			var cameraNode = scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
			cameraNode.CreateComponent<Camera>();
			Renderer.SetViewport(0, Viewport = new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null));

			// UI
			coinsText = new Text(Context);
			coinsText.HorizontalAlignment = HorizontalAlignment.Right;
			coinsText.SetFont(ResourceCache.GetFont(Assets.Fonts.Font), Graphics.Width / 20);
			UI.Root.AddChild(coinsText);
			Input.SetMouseVisible(true, false);

			// Background
			var background = new Background(Context);
			scene.AddComponent(background);
			background.Start();

			// Lights:
			var lightNode1 = scene.CreateChild();
			lightNode1.Position = new Vector3(0, -5, -40);
			lightNode1.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 120, Brightness = 1.5f });

			var lightNode2 = scene.CreateChild();
			lightNode2.Position = new Vector3(10, 15, -12);
			lightNode2.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 30.0f, CastShadows = true, Brightness = 1.5f });

			// Menu
			var startMenu = new StartMenu(Context);
			scene.AddComponent(startMenu);

			// Game logic cycle
			while (true)
			{
				await startMenu.ShowStartMenu(); //wait for "start"
				await StartGame();
			}
		}

		async Task StartGame()
		{
			UpdateCoins(0);
			player = new Player(Context);
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(player);
			var playersLife = player.Play();

			enemies = new List<Enemy>();

			for (int i = 0; i < EnemySpawningIntensivity; i++)
			{
				SpawnEnemies();
				await aircraftNode.RunActionsAsync(new DelayTime(1));
			}
			SpawnCoins();

			await playersLife;

			//game over -- explode all enemies
			foreach (var enemy in enemies)
				enemy.Explode(); 

			aircraftNode.Remove();
		}

		async void SpawnEnemies()
		{
			// Summon enemies one by one
			while (player.IsAlive)
			{
				Enemy enemy = RandomHelper.NextRandom(0, 3) == 1 ? new EnemySlotMachine(Context) : (Enemy)new EnemyBat(Context);
				var enemyNode = scene.CreateChild(nameof(Aircraft));
				enemyNode.AddComponent(enemy);
				enemies.Add(enemy);
				await enemy.Play();
				enemies.Remove(enemy);
				enemyNode.Remove();
			}
		}

		async void SpawnCoins()
		{
			while (player.IsAlive)
			{
				var coinNode = scene.CreateChild();
				coinNode.Position = new Vector3(RandomHelper.NextRandom(-2.5f, 2.5f), 4f, 0);
				var coin = new Coin(Context);
				coinNode.AddComponent(coin);
				await coin.FireAsync(false);
				coinNode.Remove();
			}
		}

		public void OnCoinCollected() => UpdateCoins(coins + 1);

		void UpdateCoins(int amount)
		{
			coins = amount;
			coinsText.Value = string.Format(CoinstFormat, coins);
		}
	}
}
