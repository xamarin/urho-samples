using Urho;

namespace SamplyGame
{
	public class EnemySkull : Enemy
	{
		public override int MaxHealth => 50;

		protected override Vector3 CollisionShapeSize => base.CollisionShapeSize * 4;

		protected override void Init()
		{
			InitialRotation = new Quaternion(0, 90, -40f);
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Enemy4);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Enemy4DD).Clone(""));
			node.SetScale(0.15f);
			node.Position = new Vector3(0f, 5f, 0f);

			// load weapons:
			node.AddComponent(new MassCross());

			base.Init();
		}
	}
}
