using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class Background : Component
	{
		Node frontTile, rearTile;

		const float BackgroundRotationX = 45f;
		const float BackgroundRotationY = 15f;
		const float BackgroundScale = 40f;
		const float BackgroundSpeed = 0.06f;
		const float FlightHeight = 10f;
		const int TreesPerTile = 200;

		public Background(Context context) : base(context) {}

		public void Start()
		{
			// Background consists of two tiles (each BackgroundScale x BackgroundScale)
			frontTile = CreateTile(0);
			rearTile = CreateTile(1);

			// Move them and swap (rotate) to be looked like the background is infinite
			RotateBackground();
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
			var cache = Application.ResourceCache;
			Node tile = Node.CreateChild();
			var planeNode = tile.CreateChild();
			planeNode.Scale = new Vector3(BackgroundScale, 0.0001f, BackgroundScale);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel(Assets.Models.Box);
			planeObject.SetMaterial(cache.GetMaterial(Assets.Materials.Grass));

			var usedCoordinates = new HashSet<Vector3>();
			for (int i = 0; i < TreesPerTile; i++)
			{
				Vector3 randomCoordinates;
				// Generate random unique coordinates for trees
				while (true)
				{
					var x = (int)RandomHelper.NextRandom(-10, 10);
					var y = (int)RandomHelper.NextRandom(-25, 25);
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
			var cache = Application.ResourceCache;
			Node treeNode = container.CreateChild();
			treeNode.Rotate(new Quaternion(0, RandomHelper.NextRandom(0, 5) * 90, 0), TransformSpace.Local);
			var model = treeNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Tree);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.TreeMaterial));
			treeNode.SetScale(RandomHelper.NextRandom(0.2f, 0.3f));
			model.CastShadows = true;
			return treeNode;
		}
	}
}
