using System;
using System.Collections.Generic;
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

		public Skies(Context c) : base(c, new ApplicationOptions { Height = 800, Width = 500, ResizableWindow = true, Orientation = ApplicationOptions.OrientationType.Portrait}) { }

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
			cameraLight.Brightness = 1.3f;

			player = new Player(Context);
			var aircraftNode = scene.CreateChild(nameof(Aircraft));
			aircraftNode.AddComponent(player);
			var playersLife = player.Play(health: 30);

			var lightNode = scene.CreateChild("AdditionalLight");
			lightNode.Position = new Vector3(1, 0, -5);
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 110.0f;
			light.CastShadows = false;
			light.Brightness = 1f;





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

		Node CreateTile(int index)
		{
			var cache = ResourceCache;
			Node tile = scene.CreateChild();
			var planeNode = tile.CreateChild("Plane");
			planeNode.Scale = new Vector3(BackgroundScale, 0.1f, BackgroundScale);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Box.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/Grass.xml"));

			// Create billboard sets (floating smoke)
			const uint numBillboardnodes = 15;
			const uint numBillboards = 3;

			for (uint i = 0; i < numBillboardnodes; ++i)
			{
				var smokeNode = tile.CreateChild("Smoke");
				smokeNode.Position = new Vector3(NextRandom(0f, 6f), NextRandom(1f, 3f), NextRandom(-22f, 4f));

				var billboardObject = smokeNode.CreateComponent<BillboardSet>();
				billboardObject.NumBillboards = numBillboards;
				billboardObject.Material = ResourceCache.GetMaterial("Materials/LitSmoke.xml");
				billboardObject.SetSorted(true);

				for (uint j = 0; j < numBillboards; ++j)
				{
					//NOTE: temp working solution. TODO: avoid using "unsafe"
					var bb = billboardObject.GetBillboardSafe(j);
					bb.Position = new Vector3(NextRandom(1, 3), NextRandom(0, 3), 0);
					bb.Size = new Vector2(NextRandom(1, 3f), NextRandom(1.5f, 3f));
					bb.Rotation = NextRandom(30, 90);
					bb.Enabled = true;
				}

				// After modifying the billboards, they need to be "commited" so that the BillboardSet updates its internals
				billboardObject.Commit();
			}


			var usedCoordinates = new HashSet<Vector3>();
			for (int i = 0; i < BackgroundScale * 4; i++)
			{
				Vector3 randomCoordinates;
				// Generate random unique coordinates for trees
				while (true)
				{
					randomCoordinates = new Vector3((int) NextRandom(-BackgroundScale/2, BackgroundScale/2), 0f, (int) NextRandom(-BackgroundScale/2, BackgroundScale/2));
					if (!usedCoordinates.Contains(randomCoordinates))
					{
						usedCoordinates.Add(randomCoordinates);
						break;
					}
				}

				var tree = CreateTree(tile);
				tree.Position = randomCoordinates;
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
