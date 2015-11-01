using System;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public abstract class Weapon : Component
	{
		protected Weapon(Context context) : base(context) { }

		public bool Inited { get; set; }

		public DateTime LastLaunchDate { get; set; }

		/// <summary>
		/// Reload duration (between two launches)
		/// </summary>
		public virtual TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.5f);

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

		public virtual void OnHit(Aircraft target, bool killed, Node bulletNode)
		{
			bulletNode.GetComponent<RigidBody>()?.SetEnabled(false);
			bulletNode.SetScale(0);
		}

		protected virtual void Init() {}

		protected Node CreateRigidBullet(bool byPlayer)
		{
			var carrier = Node;
			var bullet = carrier.Scene.CreateChild(nameof(Weapon) + GetType().Name);
			var carrierPos = carrier.Position;
			bullet.Position = carrierPos;
			var body = bullet.CreateComponent<RigidBody>();
			CollisionShape shape = bullet.CreateComponent<CollisionShape>();
			shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);
			body.SetKinematic(true);
			body.CollisionLayer = byPlayer ? Enemy.EnemyCollisionLayer : Player.PlayerAircraftCollisionLayer;
			bullet.AddComponent(new WeaponReferenceComponent(Context, this));
			return bullet;
		}

		protected abstract Task OnFire(bool byPlayer);
	}

	public class WeaponReferenceComponent : Component
	{
		public Weapon Weapon { get; set; }

		public WeaponReferenceComponent(Context context, Weapon weapon) : base(context)
		{
			Weapon = weapon;
		}
	}
}
