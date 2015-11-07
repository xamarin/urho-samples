using System.Linq;
using Urho;

namespace ShootySkies
{
	public abstract class Enemy : Aircraft
	{
		protected Enemy(Context context) : base(context) { }

		protected override CollisionLayers CollisionLayer => CollisionLayers.Enemy;

		protected override async void Init()
		{
			await Node.RunActionsAsync(new MoveBy(0.6f, new Vector3(0, -2, 0)));
			MoveRandomly();
			AttackRandomly();
		}

		async void AttackRandomly()
		{
			while (IsAlive && Node.Components.Count > 0)
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					await weapon.FireAsync(false);
					if (!IsAlive)
						return;
				}
				await Node.RunActionsAsync(new DelayTime(0.1f));
			}
		}

		async void MoveRandomly()
		{
			while (IsAlive)
			{
				var moveAction = new MoveBy(RandomHelper.NextRandom(1f, 2f), new Vector3(RandomHelper.NextRandom(-3f, 3f), RandomHelper.NextRandom(-4f, 1f), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}

		protected override void OnUpdate(SceneUpdateEventArgs args)
		{
			Node.LookAt(new Vector3(0, -3, 0), new Vector3(0, 1, -1), TransformSpace.World);
			base.OnUpdate(args);
		}
	}
}
