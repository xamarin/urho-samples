using System;
using System.Linq;
using System.Threading.Tasks;

namespace Urho.Samples
{
	public abstract class Weapon : Component
	{
		protected Weapon(Context context) : base(context) { }

		protected bool Inited { get; set; }
		
		protected DateTime LastLaunchDate { get; set; }

		/// <summary>
		/// Reload duration (between two launches)
		/// </summary>
		protected virtual TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.5f);

		public virtual int Damage => 1;

		/// <summary>
		/// The weapon is being reloaded.
		/// </summary>
		public bool IsReloading => LastLaunchDate + ReloadDuration > DateTime.Now;

		/// <summary>
		/// </summary>
		public async Task<bool> FireAsync(bool byPlayer)
		{
			if (!Inited)
			{
				Inited = true;
				Init();
			}

			if (IsReloading)
			{
				return false;
			}

			LastLaunchDate = DateTime.Now;
			await OnFire(byPlayer);
			return true;
		}

		protected virtual void Init()
		{
			SubscribeToNodeCollision(OnCollided);
		}

		protected Node CreateRigidBullet(bool byPlayer)
		{
			var carrier = Node;
			Node bullet = carrier.Scene.CreateChild(GetType().Name);
			var carrierPos = carrier.Position;
			bullet.Position = carrierPos;

			var body = bullet.CreateComponent<RigidBody>();
			CollisionShape shape = bullet.CreateComponent<CollisionShape>();
			shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);
			body.SetKinematic(true);
			body.CollisionLayer = byPlayer ? Enemy.EnemyCollisionLayer : Player.PlayerAircraftCollisionLayer;
			return bullet;
		}

		protected virtual void OnCollided(Node bullet, Aircraft target, bool killed) {}

		protected abstract Task OnFire(bool byPlayer);

		void OnCollided(NodeCollisionEventArgs args)
		{
			var node = args.Body.Node;
			var otherNode = args.OtherNode;
			if (node.Name == GetType().Name && otherNode.Name == nameof(Aircraft))
			{
				node.GetComponent<RigidBody>()?.SetEnabled(false);
				node.SetScale(0);

				var aircraft = otherNode.Components.OfType<Aircraft>().FirstOrDefault();
				if (aircraft != null)
				{
					aircraft.Health -= Damage;
					bool killed = aircraft.Health <= 0;
					if (killed)
					{
						aircraft.Explode();
					}
					OnCollided(node, aircraft, killed);
				}
			}
		}
	}
}
