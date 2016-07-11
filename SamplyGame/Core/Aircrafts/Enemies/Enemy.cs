using System.Linq;
using Urho;
using Urho.Actions;

namespace SamplyGame
{
	/// <summary>
	/// Base class for all kind of enemies
	/// </summary>
	public abstract class Enemy : Aircraft
	{
		protected override CollisionLayers CollisionLayer => CollisionLayers.Enemy;

		protected Quaternion InitialRotation { get; set; } = new Quaternion(0, 0, 0);

		protected override async void Init()
		{
			await Node.RunActionsAsync(new MoveBy(0.6f, new Vector3(0, -2, 0)));
			MoveRandomly(minX: 1, maxX: 2, minY: -3, maxY: 3, duration: 1.5f);
			StartShooting();
		}

		protected async void StartShooting()
		{
			while (IsAlive && Node.Components.Count > 0)
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					await weapon.FireAsync(false);
					if (!IsAlive)
						return;
				}
				await Node.RunActionsAsync(new DelayTime(RandomHelper.NextRandom(0.1f, 0.5f)));
			}
		}

		/// <summary>
		/// Set boundaries for random movements
		/// </summary>
		protected async void MoveRandomly(float minX, float maxX, float minY, float maxY, float duration)
		{
			while (IsAlive)
			{
				var moveAction = new MoveBy(duration, new Vector3(RandomHelper.NextRandom(minX, maxX), RandomHelper.NextRandom(minY, maxY), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}


		protected override void OnUpdate(float timeStep)
		{
			Node.LookAt(new Vector3(0, -3, 0), new Vector3(0, 1, -1), TransformSpace.World);
			Node.Rotate(InitialRotation, TransformSpace.Local);
		}
	}
}
