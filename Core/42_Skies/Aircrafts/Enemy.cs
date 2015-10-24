using System.Linq;
using System.Threading.Tasks;

namespace Urho.Samples
{
	public class Enemy : Aircraft
	{
		public const uint EnemyCollisionLayer = 4; //specific layer to ignore own bullets

		public Enemy(Context context) : base(context) {}

		protected override uint CollisionLayer => EnemyCollisionLayer;

		protected override async void Init()
		{
			var cache = Application.Current.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/f16.mdl");

			node.SetScale(0.1f);
			node.Position = new Vector3(0f, 5f, 0f);
			node.Rotation = new Quaternion(180, 180, 0);

			await node.RunActionsAsync(new MoveBy(1f, new Vector3(0, -2, 0)));
			node.AddComponent(new HeavyMissile(Context));

			MoveRandomly();
			AttackRandomly();
		}

		async void AttackRandomly()
		{
			while (IsEnabled() && !IsDeleted)
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					await weapon.FireAsync(false);
				}
			}
		}

		async void MoveRandomly()
		{
			while (IsEnabled() && !IsDeleted)
			{
				var moveAction = new MoveBy(2f, new Vector3(Sample.NextRandom(-2f, 3f), Sample.NextRandom(-1f, 1f), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}
	}
}
