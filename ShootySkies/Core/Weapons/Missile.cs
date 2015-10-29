using System;
using System.Threading;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class Missile : Weapon
	{
		public Missile(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(1);

		public override int Damage => 10;

		protected override Task OnFire(bool player)
		{
			return LaunchSingleMissile(left: RandomHelper.NextRandom(0, 2) == 1, player: player);

			// launch two missiles at the same time
			// return Task.WhenAll(LaunchSingleMissile(true, player), LaunchSingleMissile(false, player));
		}

		async Task LaunchSingleMissile(bool left, bool player)
		{
			var cache = Application.ResourceCache;
			var carrier = Node;
			var carrierPos = carrier.Position;

			var bulletNode = CreateRigidBullet(player);
			bulletNode.Position = new Vector3(carrierPos.X + 0.7f * (left ? -1 : 1), carrierPos.Y + 0.1f, carrierPos.Z);
			var bulletModelNode = bulletNode.CreateChild();

			var model = bulletModelNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Box.mdl");

			bulletModelNode.Scale = new Vector3(1f, 2f, 1f) / 2f;
			bulletNode.SetScale(0.2f);

			// Trace-effect using particles
			var particleEmitter = bulletNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/MissileTrace.pex");

			// Route (Bezier)
			float directionY = player ? 1 : -1;
			float directionX = left ? -1 : 1;
			var moveMissileAction = new BezierBy(1.2f, new BezierConfig
				{
					ControlPoint1 = new Vector3(0, 2f * directionY, 0),
					ControlPoint2 = new Vector3(-4 * directionX, 8 * directionY, 0),
					EndPosition = new Vector3(2 * directionX, 12 * directionY, 0),
				});

			await bulletNode.RunActionsAsync(
				new EaseIn(moveMissileAction, 2), // move
				new CallFunc(() => bulletNode.SetScale(0f)), //collapse
				new DelayTime(2f)); //a delay to leave the trace effect

			//remove the missile from the scene.
			bulletNode.Remove();
		}

		protected override async void OnCollided(Node missile, Aircraft target, bool killed)
		{
			// show a small explosion (it doesn't mean the target is killed)
			var cache = Application.Current.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.SetScale(0.5f);
			explosionNode.Position = target.Node.WorldPosition;
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/MissileTrace.pex");
			ScaleBy scaleBy = new ScaleBy(0.2f, 0.1f);
			var syn1 = SynchronizationContext.Current;
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			var syn2 = SynchronizationContext.Current;
			explosionNode.Remove();
		}
	}
}
