using Urho;

namespace SamplyGame
{
	public class EnemySlotMachine : Enemy
	{
		public override int MaxHealth => 50;

		protected override void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Enemy2);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Enemy2).Clone(""));
			node.SetScale(RandomHelper.NextRandom(0.85f, 1f));
			node.Position = new Vector3(0f, 5f, 0f);

			// load weapons:
			node.AddComponent(new Joysticks());

			base.Init();
		}
	}
}
