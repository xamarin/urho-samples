using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class ShootySkiesGame : Application
	{
		Scene scene;
		Node frontTile, rearTile;
		Player player;
		List<Enemy> enemies;

		const float BackgroundRotationX = 45f;
		const float BackgroundRotationY = 15f;
		const float BackgroundScale = 40f;
		const float BackgroundSpeed = 0.08f;
		const float FlightHeight = 10f;
		const int TreesPerTile = 200;

		public ShootySkiesGame(Context c) : base(c, new ApplicationOptions { Height = 800, Width = 500, Orientation = ApplicationOptions.OrientationType.Portrait }) { }

		public override void Start()
		{
			base.Start();
			CreateScene();
			SubscribeToKeyDown(e =>
				{
					if (e.Key == Key.Esc)
						Engine.Exit();
				});
		}

		async void CreateScene()
		{
			scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));

			var cameraNode = scene.CreateChild();
			cameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
			cameraNode.CreateComponent<Camera>();
			Renderer.SetViewport(0, new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null));

			// Background consists of two tiles (each BackgroundScale x BackgroundScale)
			frontTile = CreateTile(0);
			rearTile = CreateTile(1);

			// Move them and swap (rotate) to be looked like the background is infinite
			RotateBackground();
			
			// Lights:

			var lightNode1 = scene.CreateChild();
			lightNode1.Position = new Vector3(0, -5, -40);
			lightNode1.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 120, Brightness = 1.4f });

			var lightNode2 = scene.CreateChild();
			lightNode2.Position = new Vector3(10, 15, -12);
			lightNode2.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 30.0f, CastShadows = true, Brightness = 1.7f });

			var startMenu = new StartMenu(Context);
			scene.AddComponent(startMenu);

			while (true)
			{
				await startMenu.ShowStartMenu(); //wait for "start"
				await StartGame();
			}
		}

		async Task StartGame()
		{
			player = new Player(Context);
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(player);
			var playersLife = player.Play();

			enemies = new List<Enemy>();

			SummonEnemies();
			await aircraftNode.RunActionsAsync(new DelayTime(1));
			SummonEnemies();
			await aircraftNode.RunActionsAsync(new DelayTime(1));
			SummonEnemies();

			await playersLife;

			//game over -- explode all enemies
			foreach (var enemy in enemies)
				enemy.Explode(); 

			aircraftNode.Remove();
		}

		async void SummonEnemies()
		{
			// Summon enemies one by one
			while (player.IsAlive)
			{
				var enemy = (RandomHelper.NextRandom(0, 3) == 1) ? (Enemy)new EnemySlotMachine(Context) : new EnemyBat(Context);
				var enemyNode = scene.CreateChild(nameof(Aircraft));
				enemyNode.AddComponent(enemy);
				enemies.Add(enemy);
				await enemy.Play();
				enemies.Remove(enemy);
				enemyNode.Remove();
			}
		}

		/// <summary>
		/// The background actually is not infinite, it consists of two tiles which change places
		/// </summary>
		async void RotateBackground()
		{
			while (true)
			{
				// calculate positions using Law of sines
				var x = BackgroundScale*(float) Math.Sin((90 - BackgroundRotationX)*MathHelper.Pi/180);
				var y = BackgroundScale*(float) Math.Sin(BackgroundRotationX*MathHelper.Pi/180) + FlightHeight;

				var moveTo = x + 1f; //a small adjusment to hide that gap between two tiles
				var h = (float)Math.Tan(BackgroundRotationX * MathHelper.Pi / 180) * moveTo;
				await Task.WhenAll(
					frontTile.RunActionsAsync(new MoveBy(1 / BackgroundSpeed, new Vector3(0, -moveTo, -h))),
					rearTile.RunActionsAsync(new MoveBy(1 / BackgroundSpeed, new Vector3(0, -moveTo, -h))));

				//switch tiles
				var tmp = frontTile;
				frontTile = rearTile;
				rearTile = tmp;

				rearTile.Position = new Vector3(0, x, y);
			}
		}

		Node CreateTile(int index)
		{
			var cache = ResourceCache;
			Node tile = scene.CreateChild();
			var planeNode = tile.CreateChild();
			planeNode.Scale = new Vector3(BackgroundScale, 0.0001f, BackgroundScale);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Box.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/Grass.xml"));

			var usedCoordinates = new HashSet<Vector3>();
			for (int i = 0; i < TreesPerTile; i++)
			{
				Vector3 randomCoordinates;
				// Generate random unique coordinates for trees
				while (true)
				{
					var x = (int) RandomHelper.NextRandom(-10, 10);
					var y = (int) RandomHelper.NextRandom(-25, 25);
					randomCoordinates = new Vector3(x, 0f, y);
					if (!usedCoordinates.Contains(randomCoordinates))
					{
						usedCoordinates.Add(randomCoordinates);
						break;
					}
				}

				var tree = CreateTree(tile);
				tree.Position = randomCoordinates;
			}
			tile.Rotate(new Quaternion(270 + BackgroundRotationX, 0, 0), TransformSpace.Local);
			tile.RotateAround(new Vector3(0, 0, 0), new Quaternion(0, BackgroundRotationY, 0), TransformSpace.Local);

			switch (index)
			{
				case 0:
					tile.Position = new Vector3(0, 0, FlightHeight);
					break;
				case 1:
					var x = BackgroundScale * (float)Math.Sin((90 - BackgroundRotationX) * MathHelper.Pi / 180);
					var y = BackgroundScale * (float)Math.Sin(BackgroundRotationX * MathHelper.Pi / 180) + FlightHeight;
					tile.Position = new Vector3(0, x + 0.01f, y);
					break;
			}

			return tile;
		}

		Node CreateTree(Node container)
		{
			var cache = ResourceCache;
			Node treeNode = container.CreateChild();
			treeNode.Rotate(new Quaternion(0, RandomHelper.NextRandom(0, 5) * 90, 0), TransformSpace.Local);
			var model = treeNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Tree.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/TreeMaterial.xml"));
			treeNode.SetScale(RandomHelper.NextRandom(0.2f, 0.3f));
			model.CastShadows = true;
			return treeNode;
		}
	}
}
