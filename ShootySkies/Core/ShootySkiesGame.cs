using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class ShootySkiesGame : Application
	{
		Scene scene;
		Node frontTile, rearTile;
		Player player;

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

			var cameraNode = scene.CreateChild("Camera");
			cameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
			cameraNode.CreateComponent<Camera>();
			Renderer.SetViewport(0, new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null));

			// Background consists of two tiles (each BackgroundScale x BackgroundScale)
			frontTile = CreateTile(0);
			rearTile = CreateTile(1);

			// Move them and swap (rotate) to be looked like the background is infinite
			RotateBackground();

			player = new Player(Context);
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(player);
			var playersLife = player.Play();

			// UI:

			var textBlock = new Text(Context);
			textBlock.HorizontalAlignment = HorizontalAlignment.Right;
			textBlock.Value = "points: 628";
			textBlock.SetFont(ResourceCache.GetFont("Fonts/BlueHighway.ttf"), 22);
			UI.Root.AddChild(textBlock);

			// Lights:

			var lightNode1 = scene.CreateChild("Light1");
			lightNode1.Position = new Vector3(0, -5, -40);
			lightNode1.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 120, Brightness = 1.4f });

			var lightNode2 = scene.CreateChild("Light2");
			lightNode2.Position = new Vector3(10, 15, -12);
			lightNode2.AddComponent(new Light(Context) { LightType = LightType.Point, Range = 30.0f, CastShadows = true, Brightness = 1.7f });

			SummonEnemies();
			SummonEnemies();
			SummonEnemies();

			await playersLife;
			aircraftNode.Remove();
			//game over
		}

		async void SummonEnemies()
		{
			// Summon enemies one by one
			while (true)
			{
				var enemy = new EnemyBat(Context);
				var enemyNode = scene.CreateChild(nameof(Aircraft));
				await enemyNode.RunActionsAsync(new DelayTime(RandomHelper.NextRandom(0.2f, 1f)));
				enemyNode.AddComponent(enemy);
				await enemy.Play();
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
			var planeNode = tile.CreateChild("Plane");
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
