using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Urho2D;
using Urho.Audio;

namespace SamplyGame
{
	public class Missile : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(3);

		public override int Damage => 12;

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
			bulletNode.Position = new Vector3(carrierPos.X + 0.4f * (left ? -1 : 1), carrierPos.Y + 0.3f, carrierPos.Z);
			var bulletModelNode = bulletNode.CreateChild();

			bulletModelNode.Scale = new Vector3(1f, 2f, 1f) / 2.5f;
			bulletNode.SetScale(0.3f);

			// Trace-effect using particles
			bulletNode.CreateComponent<ParticleEmitter2D>().Effect = cache.GetParticleEffect2D(Assets.Particles.MissileTrace);
			bulletNode.CreateComponent<ParticleEmitter2D>().Effect = cache.GetParticleEffect2D(Assets.Particles.Explosion);

			// Route (Bezier)
			float directionY = player ? 1 : -1;
			float directionX = left ? -1 : 1;
			var moveMissileAction = new BezierBy(1.0f, new BezierConfig
				{
					ControlPoint1 = new Vector3(-directionX, 2f * directionY, 0),
					ControlPoint2 = new Vector3(RandomHelper.NextRandom(-2, 2) * directionX, 4 * directionY, 0),
					EndPosition = new Vector3(RandomHelper.NextRandom(-1, 1) * directionX, 12 * directionY, 0),
				});

			await bulletNode.RunActionsAsync(
				new EaseIn(moveMissileAction, 1), // move
				new CallFunc(() => bulletNode.SetScale(0f)), //collapse
				new DelayTime(2f)); //a delay to leave the trace effect

			//remove the missile from the scene.
			bulletNode.Remove();
		}

		public override async void OnHit(Aircraft target, bool killed, Node bulletNode)
		{
			// show a small explosion when the missile reaches an aircraft. 
			base.OnHit(target, killed, bulletNode);
			var cache = Application.ResourceCache;
			var explosionNode = Scene.CreateChild();

			// play "boom" sound
			SoundSource soundSource = explosionNode.CreateComponent<SoundSource>();
			soundSource.Play(Application.ResourceCache.GetSound(Assets.Sounds.SmallExplosion));
			soundSource.Gain = 0.2f;

			explosionNode.Position = target.Node.WorldPosition;
			explosionNode.SetScale(1f);
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D(Assets.Particles.MissileTrace);
			var scaleAction = new ScaleTo(0.5f, 0f);
			await explosionNode.RunActionsAsync(scaleAction, new DelayTime(0.5f));
			explosionNode.Remove();
		}
	}
}
