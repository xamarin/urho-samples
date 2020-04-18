using System;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;

namespace SamplyGame
{
	public class SmallPlate : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.45f);

		public override int Damage => 10;

		protected async override Task OnFire(bool byPlayer)
		{
			var cache = Application.ResourceCache;

			var bulletNode = CreateRigidBullet(byPlayer, Vector3.One / 3);
			bulletNode.Rotation = new Quaternion(310, 0, 0);
			bulletNode.SetScale(1f);

			var model = bulletNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Enemy3weapon);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Enemy3weapon));
			Launch(bulletNode);
		}

		async void Launch(Node bulletNode)
		{
			await bulletNode.RunActionsAsync(
				new MoveTo(3f, new Vector3(RandomHelper.NextRandom(-6f, 6f), -6, 0)),
				new CallFunc(() => bulletNode.SetScale(0f)));

			//remove the bullet from the scene.
			bulletNode.Remove();
		}
	}
}
