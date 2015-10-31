using Urho;

namespace ShootySkies
{
	public class EnemySlotMachine : Enemy
	{
		public EnemySlotMachine(Context context) : base(context) { }

		public override int MaxHealth => 30;

		protected override void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Enemy2.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/Enemy2.xml").Clone(""));

			node.SetScale(RandomHelper.NextRandom(0.85f, 1f));
			node.Position = new Vector3(0f, 5f, 0f);

			node.AddComponent(new Joysticks(Context));
			base.Init();
		}
	}
}
