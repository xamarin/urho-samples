using System;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public abstract class Weapon : Component
	{
		protected Weapon(Context context) : base(context) { }

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

		protected Node CreateRigidBullet(bool byPlayer, Vector3 collisionBox)
		{
			var carrier = Node;
			var bullet = carrier.Scene.CreateChild(nameof(Weapon) + GetType().Name);
			var carrierPos = carrier.Position;
			bullet.Position = carrierPos;
			var body = bullet.CreateComponent<RigidBody>();
			CollisionShape shape = bullet.CreateComponent<CollisionShape>();
			shape.SetBox(collisionBox, Vector3.Zero, Quaternion.Identity);
			body.SetKinematic(true);
			body.CollisionLayer = byPlayer ? (uint)CollisionLayers.Enemy : (uint)CollisionLayers.Player;
			bullet.AddComponent(new WeaponReferenceComponent(Context, this));
			return bullet;
		}

		protected Node CreateRigidBullet(bool byPlayer)
		{
			return CreateRigidBullet(byPlayer, Vector3.One);
		}

		protected abstract Task OnFire(bool byPlayer);
	}

	/// <summary>
	/// A component that should be attached to bullets' nodes in order to have a link to the Weapon
	/// </summary>
	public class WeaponReferenceComponent : Component
	{
		public Weapon Weapon { get; private set; }

		public WeaponReferenceComponent(Context context, Weapon weapon) : base(context)
		{
			Weapon = weapon;
		}
	}
}
