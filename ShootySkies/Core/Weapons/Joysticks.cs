using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class Joysticks : Weapon
	{
		public Joysticks(Context context) : base(context) { }

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(2f);

		public override int Damage => 10;

		protected override Task OnFire(bool byPlayer)
		{
			const int joysticksCount = 12;
			const float length = 10f;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < joysticksCount; i++)
			{
				var angle = 360 / joysticksCount * i * MathHelper.Pi / 180; //angle per joystick (in radians)
				tasks.Add(Fire(new Vector3((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length, 0), byPlayer));
			}
			return Task.WhenAll(tasks);
		}

		async Task Fire(Vector3 direction, bool byPlayer)
		{
			var cache = Application.ResourceCache;

			var bulletNode = CreateRigidBullet(byPlayer);
			bulletNode.Rotation = new Quaternion(130, 0, 0);
			var b = bulletNode.CreateChild();

			var model = b.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.SMWeapon);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.SMWeapon));

			await bulletNode.RunActionsAsync(
				new MoveBy(5f, direction),
				new CallFunc(() => b.SetScale(0f))); //collapse);

			//remove the bullet from the scene.
			bulletNode.Remove();
		}
	}
}
