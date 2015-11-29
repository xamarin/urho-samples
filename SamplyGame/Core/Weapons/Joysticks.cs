using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;

namespace SamplyGame
{
	public class Joysticks : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(2f);

		public override int Damage => 10;

		protected override Task OnFire(bool byPlayer)
		{
			const int joysticksCount = 12;
			const float length = 10f;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < joysticksCount; i++)
			{
				var angle = MathHelper.DegreesToRadians(360 / joysticksCount * i); //angle per joystick (in radians)
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
			bulletNode.Rotation = new Quaternion(130, 0, 0);
			bulletNode.SetScale(0.8f);

			var model = bulletNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.SMWeapon);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.SMWeapon));

			await bulletNode.RunActionsAsync(
				new MoveBy(5f, direction),
				new CallFunc(() => bulletNode.SetScale(0f))); //collapse);

			//remove the bullet from the scene.
			bulletNode.Remove();
		}
	}
}
