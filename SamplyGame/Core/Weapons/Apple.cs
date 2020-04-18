using System;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Audio;

namespace SamplyGame
{
	public class Apple : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(0.1f);

		public override int Damage => 0;

		protected override async Task OnFire(bool byPlayer)
		{
			var cache = Application.ResourceCache;
			var node = CreateRigidBullet(byPlayer);
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Apple);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Apple));
			node.SetScale(1);
			node.Rotation = new Quaternion(-40, 0, 0);
			await node.RunActionsAsync(
				new Urho.Actions.Parallel(
					new MoveBy(duration: 3f, position: new Vector3(0, 10 * (byPlayer ? 1 : -1), 0)),
					new RotateBy(duration: 3f, deltaAngleX: 0, deltaAngleY: 360 * 5, deltaAngleZ: 0)));
			node.Remove();
		}

		public override void OnHit(Aircraft target, bool killed, Node bulletNode)
		{
			var soundSource = Node.CreateComponent<SoundSource>();
			soundSource.Gain = 0.1f;
			soundSource.Play(Application.ResourceCache.GetSound(Assets.Sounds.Powerup));
			base.OnHit(target, killed, bulletNode);
			((SamplyGame)Application).OnCoinCollected();
		}
	}
}
