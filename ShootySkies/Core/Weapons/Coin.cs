using System;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class Coin : Weapon
	{
		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.1f);
		public override int Damage => 0;

		public Coin(Context context) : base(context) {}

		protected override Task OnFire(bool byPlayer)
		{
			var cache = Application.ResourceCache;
			var node = CreateRigidBullet(byPlayer);
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Coin);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Coin));

			node.SetScale(1);
			node.Rotation = new Quaternion(-40, 0, 0);
			return node.RunActionsAsync(
				new Urho.Parallel(
					new MoveBy(3f, new Vector3(0, 10 * (byPlayer ? 1 : -1), 0)),
					new RotateBy(3f, 0, 360 * 5, 0)));
		}

		protected override void OnHit(Aircraft target, bool killed)
		{
			((ShootySkiesGame)Application).OnCoinCollected();
		}
	}
}
