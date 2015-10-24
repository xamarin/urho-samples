using System;
using System.Threading.Tasks;

namespace Urho.Samples
{
	public class Skies : Sample
	{
		Scene scene;
		Node frontTile, rearTile;
		Player player;

		const float BackgroundRotationX = 15f;
		const float BackgroundRotationY = 20f;
		const float BackgroundScale = 20f;
		const float BackgroundSpeed = 0.1f;
		const float FlightHeight = 9f;

		public Skies(Context c) : base(c) { }

		public override void Start()
		{
			base.Start();
			CreateScene();
			IsLogoVisible = false;
		}

		async void CreateScene()
		{
			scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			var physics = scene.CreateComponent<PhysicsWorld>();
			physics.SetGravity(new Vector3(0, 0, 0));

			CameraNode = scene.CreateChild("Camera");
			CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
			CameraNode.CreateComponent<Camera>();
			Renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));

			// Background consists of two tiles (each BackgroundScale x BackgroundScale)
			frontTile = CreateTile(0);
			rearTile = CreateTile(1);

			// Move them and swap (rotate) to be looked like the background is infinite
			RotateBackground();

			var cameraLight = CameraNode.CreateComponent<Light>();
			cameraLight.LightType = LightType.Point;
			cameraLight.Range = 20.0f;
			cameraLight.CastShadows = true;

			player = new Player(Context);
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(player);
			var playersLife = player.Play(health: 30);

			var lightNode = scene.CreateChild("AdditionalLight");
			lightNode.Position = new Vector3(2, 0, 2);
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 10.0f;
			light.CastShadows = true;
			
			SummonEnemies();
			await playersLife;
			//game over
		}

		async void SummonEnemies()
		{
			// Summon enemies one by one
			while (true)
			{
				var enemy = new Enemy(Context);
				var enemyNode = scene.CreateChild(nameof(Aircraft));
				enemyNode.AddComponent(enemy);
				await enemy.Play(health: 30);
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
				var x = BackgroundScale * (float)Math.Sin((90 - BackgroundRotationX) * MathHelper.Pi / 180);
				var y = BackgroundScale * (float)Math.Sin(BackgroundRotationX * MathHelper.Pi / 180) + FlightHeight;

				var moveTo = x - 0.001f; //a small adjusment to hide that gap between two tiles
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

		void SetupViewport()
		{
		}

		Node CreateTile(int index)
		{
			var cache = ResourceCache;
			Node tile = scene.CreateChild();
			var planeNode = tile.CreateChild("Plane");
			planeNode.Scale = new Vector3(BackgroundScale, 0.1f, BackgroundScale);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Box.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/Grass.xml"));

			for (int i = 0; i < BackgroundScale * 4; i++)
			{
				var tree = CreateTree(tile);
				tree.Position = new Vector3(
					x: (int)NextRandom(-BackgroundScale / 2, BackgroundScale / 2), y: 0, 
					z: (int)NextRandom(-BackgroundScale / 2, BackgroundScale / 2));
			}
			tile.Rotate(new Quaternion(270 + BackgroundRotationX, BackgroundRotationY, 0), TransformSpace.Local);

			switch (index)
			{
				case 0:
					tile.Position = new Vector3(0, 0, FlightHeight);
					break;
				case 1:
					var x = BackgroundScale * (float)Math.Sin((90 - BackgroundRotationX) * MathHelper.Pi / 180);
					var y = BackgroundScale * (float)Math.Sin(BackgroundRotationX * MathHelper.Pi / 180) + FlightHeight;
					tile.Position = new Vector3(0, x, y);
					break;
			}

			return tile;
		}

		Node CreateTree(Node container)
		{
			var cache = ResourceCache;
			Node treeNode = container.CreateChild();
			var model = treeNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Box.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));
			treeNode.Scale = new Vector3(1f, 4f, 1f);
			model.CastShadows = true;
			return treeNode;
		}

		protected override void OnUpdate(float timeStep)
		{
			//SimpleMoveCamera3D(timeStep);
		}

		protected override void OnSceneUpdate(float timeStep, Scene scene)
		{
			//override Sample's behavior by no-op
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.Hidden;
	}
}
