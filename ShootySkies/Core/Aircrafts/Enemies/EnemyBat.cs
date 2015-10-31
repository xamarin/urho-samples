using Urho;

namespace ShootySkies
{
	public class EnemyBat : Enemy
	{
		public EnemyBat(Context context) : base(context) {}

		public override int MaxHealth => 20;

		protected override void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Enemy1);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Enemy1).Clone(""));

			node.SetScale(RandomHelper.NextRandom(0.5f, 0.8f));
			node.Position = new Vector3(0f, 5f, 0f);
		
			node.AddComponent(new HeavyMissile(Context));
			base.Init();
		}
	}
}
