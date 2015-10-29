using System;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public class HeavyMissile : Weapon
	{
		public HeavyMissile(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(3);

		public override int Damage => 20;

		protected override async Task OnFire(bool player)
		{
			var cache = Application.ResourceCache;

			var bulletNode = CreateRigidBullet(player);
			var bulletModelNode = bulletNode.CreateChild();

			var model = bulletModelNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Box.mdl");
			bulletModelNode.SetScale(2f);
			bulletModelNode.Rotate(new Quaternion(45, 0, 0), TransformSpace.Local);

			bulletNode.SetScale(0.2f);

			// Trace-effect using particles
			var particleEmitter = bulletNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/Explosion.pex");

			// Route (Bezier)
			float direction = player ? 1 : -1;
			var moveMissileAction = new BezierBy(3f, new BezierConfig
				{
					ControlPoint1 = new Vector3(0, 3f * direction, 0),
					ControlPoint2 = new Vector3(RandomHelper.NextRandom(-3f, 3f), 5 * direction, 0),
					EndPosition = new Vector3(0, 8 * direction, 0),//to launch "to" point
				});

			var bulletRotationTask = bulletModelNode.RunActionsAsync(new RotateBy(3f, 0, 1000, 0));
			var bulletMoveTask = bulletNode.RunActionsAsync(new EaseOut(moveMissileAction, 1), new DelayTime(2f)); //a delay to leave the trace effect
			await Task.WhenAll(bulletRotationTask, bulletMoveTask);

			//remove the missile from the scene.
			bulletNode.Remove();
		}

		protected override async void OnCollided(Node missile, Aircraft target, bool killed)
		{
			// show a small explosion (it doesn't mean the target is killed)
			var cache = Application.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.Position = target.Node.WorldPosition;
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/HeavyMissileExplosion.pex");
			ScaleBy scaleBy = new ScaleBy(0.2f, 0.1f);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			explosionNode.Remove();
		}
	}
}
