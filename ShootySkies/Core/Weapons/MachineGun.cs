using System;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class MachineGun : Weapon
	{
		const float GunOffsetSize = 0.15f; //accuracy (lower - better)
		float currentGunOffset = -GunOffsetSize;

		public MachineGun(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.1f);

		public override int Damage => 2;
		
		protected override async Task OnFire(bool player)
		{
			var cache = Application.ResourceCache;
			currentGunOffset += GunOffsetSize;
			if (currentGunOffset > GunOffsetSize)
				currentGunOffset = -GunOffsetSize;

			var bulletNode = CreateRigidBullet(player);
			bulletNode.Translate(new Vector3(currentGunOffset, 0, 0), TransformSpace.Local);

			var model = bulletNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Box.mdl");
			var mat = cache.GetMaterial("Materials/MachineGun.xml");
			model.SetMaterial(mat);
			bulletNode.Rotate(new Quaternion(0, -40, 0), TransformSpace.Local);

			bulletNode.Scale = new Vector3(0.1f, 0.3f, 0.1f);

			await bulletNode.RunActionsAsync(
				new MoveBy(0.6f, new Vector3(0, 10, 0)*(player ? 1 : -1)),
				new CallFunc(() => bulletNode.SetScale(0f))); //collapse);

			//remove the bullet from the scene.
			bulletNode.Remove();
		}
	}
}
