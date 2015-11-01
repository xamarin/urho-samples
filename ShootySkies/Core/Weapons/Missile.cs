using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class Missile : Weapon
	{
		public Missile(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(3);

		public override int Damage => 10;

		protected override async Task OnFire(bool player)
		{
			var tasks = new List<Task>();
			for (int i = 0; i < 6; i++)
			{
				tasks.Add(LaunchSingleMissile(i % 2 == 0, player: player));
				await Node.RunActionsAsync(new DelayTime(0.2f));
			}
			await Task.WhenAll(tasks);
		}

		async Task LaunchSingleMissile(bool left, bool player)
		{
			var cache = Application.ResourceCache;
			var carrier = Node;
			var carrierPos = carrier.Position;

			var bulletNode = CreateRigidBullet(player);
			bulletNode.Position = new Vector3(carrierPos.X + 0.7f * (left ? -1 : 1), carrierPos.Y + 0.1f, carrierPos.Z);
			var bulletModelNode = bulletNode.CreateChild();

			//var model = bulletModelNode.CreateComponent<StaticModel>();
			//model.Model = cache.GetModel(Assets.Models.Box);

			bulletModelNode.Scale = new Vector3(1f, 2f, 1f) / 2.5f;
			bulletNode.SetScale(0.3f);

			// Trace-effect using particles
			var particleEmitter = bulletNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D(Assets.Particles.MissileTrace);

			// Route (Bezier)
			float directionY = player ? 1 : -1;
			float directionX = left ? -1 : 1;
			var moveMissileAction = new BezierBy(1.0f, new BezierConfig
				{
					ControlPoint1 = new Vector3(RandomHelper.NextRandom(-2, 2) * directionX, 2f * directionY, 0),
					ControlPoint2 = new Vector3(RandomHelper.NextRandom(-2, 2) * directionX, 8 * directionY, 0),
					EndPosition = new Vector3(RandomHelper.NextRandom(-2, 2) * directionX, 12 * directionY, 0),
				});

			await bulletNode.RunActionsAsync(
				new EaseIn(moveMissileAction, 1), // move
				new CallFunc(() => bulletNode.SetScale(0f)), //collapse
				new DelayTime(2f)); //a delay to leave the trace effect

			//remove the missile from the scene.
			bulletNode.Remove();
		}
	}
}
