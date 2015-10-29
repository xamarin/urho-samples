using System.Linq;
using Urho;

namespace ShootySkies
{
	public class EnemyBat : Aircraft
	{
		public const uint EnemyCollisionLayer = 4; //specific layer to ignore own bullets

		public EnemyBat(Context context) : base(context) {}

		protected override uint CollisionLayer => EnemyCollisionLayer;

		public override int MaxHealth => 30;

		protected override async void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Enemy1.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/Enemy1.xml").Clone(""));

			node.SetScale(RandomHelper.NextRandom(0.7f, 0.9f));
			node.Position = new Vector3(0f, 5f, 0f);
			node.Rotation = new Quaternion(0, 0, 0);

			await node.RunActionsAsync(new MoveBy(1f, new Vector3(0, -2, 0)));
			node.AddComponent(new HeavyMissile(Context));

			MoveRandomly();
			AttackRandomly();
		}

		async void AttackRandomly()
		{
			while (IsAlive)
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					await weapon.FireAsync(false);
					if (!IsAlive)
						return;
				}
			}
		}

		async void MoveRandomly()
		{
			while (IsAlive)
			{
				var moveAction = new MoveBy(2f, new Vector3(RandomHelper.NextRandom(-3f, 3f), RandomHelper.NextRandom(-4f, 1f), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}
	}
}
