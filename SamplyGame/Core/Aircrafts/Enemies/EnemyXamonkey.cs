using Urho;
using Urho.Actions;

namespace SamplyGame
{
	public class EnemyXamonkey : Enemy
	{
		public override int MaxHealth => 30;
		protected override Vector3 CollisionShapeSize => base.CollisionShapeSize * 6;

		protected override async void Init()
		{
			InitialRotation = new Quaternion(180, 270, -30);
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Monkey);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Monkey).Clone(""));
			node.SetScale(RandomHelper.NextRandom(0.1f, 0.12f));
			node.Position = new Vector3(0f, 5f, 0f);

			// load weapons:
			node.AddComponent(new XamarinCube());

			node.Position = new Vector3(3 * (RandomHelper.NextBoolRandom() ? 1 : -1), RandomHelper.NextRandom(0, 2), 0);
			await Node.RunActionsAsync(new MoveTo(1f, new Vector3(RandomHelper.NextRandom(-2, 2), RandomHelper.NextRandom(2, 4), 0)));

			MoveRandomly(minX: -2f, maxX: 2f, minY: -1f, maxY: 1f, duration: 0.5f);
			StartShooting();
		}
	}
}
