using System;
using System.Threading.Tasks;

namespace Urho.Samples
{
	public class MachineGun : Weapon
	{
		public MachineGun(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.2f);

		protected override async Task OnFire(bool player)
		{
			var cache = Application.Current.ResourceCache;

			var bulletNode = CreateRigidBullet(player);
			var model = bulletNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Sphere.mdl");
			bulletNode.Scale = new Vector3(0.1f, 0.2f, 0.1f);

			await bulletNode.RunActionsAsync(new MoveBy(0.7f, new Vector3(0, 10, 0) * (player ? 1 : -1)));

			//remove the bullet from the scene.
			bulletNode.Remove();
		}

		protected override void OnCollided(Node bullet, Aircraft target, bool killed)
		{
			//TODO: some "glow" effect
		}
	}
}
