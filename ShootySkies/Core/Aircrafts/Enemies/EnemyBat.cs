using System.Linq;
using Urho;

namespace ShootySkies
{
	public class EnemyBat : Enemy
	{
		public EnemyBat(Context context) : base(context) {}

		public override int MaxHealth => 30;

		protected override void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Enemy1.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/Enemy1.xml").Clone(""));

			node.SetScale(RandomHelper.NextRandom(0.7f, 0.9f));
			node.Position = new Vector3(0f, 5f, 0f);
			node.Rotation = new Quaternion(0, 0, 0);
		
			node.AddComponent(new HeavyMissile(Context));
			base.Init();
		}
	}
}
