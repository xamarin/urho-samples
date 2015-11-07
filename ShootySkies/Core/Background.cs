using System;
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
		const float BackgroundSpeed = 0.05f;
		const float FlightHeight = 10f;

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
				var x = BackgroundScale * (float)Math.Sin(MathHelper.DegreesToRadians(90 - BackgroundRotationX));
				var y = BackgroundScale * (float)Math.Sin(MathHelper.DegreesToRadians(BackgroundRotationX)) + FlightHeight;

				var moveTo = x + 1f; //a small adjusment to hide that gap between two tiles
				var h = (float)Math.Tan(MathHelper.DegreesToRadians(BackgroundRotationX)) * moveTo;
				await Task.WhenAll(frontTile.RunActionsAsync(new MoveBy(1 / BackgroundSpeed, new Vector3(0, -moveTo, -h))),
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

			var size = BackgroundScale/2;
			for (float i = -size; i < size; i+=2f)
			{
				for (float j = -size; j < size; j+=2.2f)
				{
					var tree = CreateTree(tile);
					tree.Position = new Vector3(i + RandomHelper.NextRandom(-0.5f, 0.5f), 0, j);
				}
			}

			tile.Rotate(new Quaternion(270 + BackgroundRotationX, 0, 0), TransformSpace.Local);
			tile.RotateAround(new Vector3(0, 0, 0), new Quaternion(0, BackgroundRotationY, 0), TransformSpace.Local);
			var tilePosX = BackgroundScale * (float)Math.Sin(MathHelper.DegreesToRadians(90 - BackgroundRotationX));
			var tilePosY = BackgroundScale * (float)Math.Sin(MathHelper.DegreesToRadians(BackgroundRotationX));
			tile.Position = new Vector3(0, (tilePosX + 0.01f) * index, tilePosY * index + FlightHeight);
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
