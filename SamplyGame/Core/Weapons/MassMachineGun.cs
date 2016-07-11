using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;

namespace SamplyGame
{
	public class MassMachineGun : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(1f);

		public override int Damage => 4;

		protected override Task OnFire(bool byPlayer)
		{
			const int bulletsCount = 18;
			const float length = 10f;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < bulletsCount; i++)
			{
				var angle = MathHelper.DegreesToRadians(360 / bulletsCount * i); //angle per bullet (in radians)
				//x^2 + y^2 = length^2 (Equation of Circle):
				var x = (float) Math.Cos(angle) * length;
				var y = (float) Math.Sin(angle) * length;
				tasks.Add(Fire(new Vector3(x, y, 0), byPlayer));
			}
			return Task.WhenAll(tasks);
		}

		async Task Fire(Vector3 direction, bool byPlayer)
		{
			var cache = Application.ResourceCache;

			var bulletNode = CreateRigidBullet(byPlayer);
			bulletNode.SetScale(0.15f);

			var model = bulletNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Box);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.MachineGun));

			await bulletNode.RunActionsAsync(
				new MoveBy(1f, direction),
				new CallFunc(() => bulletNode.SetScale(0f)));

			//remove the bullet from the scene.
			bulletNode.Remove();
		}
	}
}
