using System;
using System.Threading.Tasks;
using Urho;
using Urho.Urho2D;
using Urho.Actions;
using Urho.Audio;

namespace SamplyGame
{
	public class XamarinCube : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(3);

		public override int Damage => 20;

		protected override async Task OnFire(bool player)
		{
			var cache = Application.ResourceCache;

			var bulletNode = CreateRigidBullet(player);
			var bulletModelNode = bulletNode.CreateChild();

			var model = bulletModelNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Box);
			model.SetMaterial(Material.FromImage(Assets.Textures.Xamarin));
			bulletModelNode.SetScale(2f);
			bulletModelNode.Rotate(new Quaternion(45, 0, 0), TransformSpace.Local);
			bulletNode.SetScale(RandomHelper.NextRandom(0.15f, 0.2f));

			// Trace-effect using particles
			var trace = bulletNode.CreateChild();
			trace.SetScale(2f);
			var particleEmitter = trace.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D(Assets.Particles.Explosion);

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
		
		public override async void OnHit(Aircraft target, bool killed, Node bulletNode)
		{
			// show a small explosion when the cube reaches an aircraft. 
			base.OnHit(target, killed, bulletNode);
			var cache = Application.ResourceCache;
			var explosionNode = Scene.CreateChild();
			SoundSource soundSource = explosionNode.CreateComponent<SoundSource>();
			soundSource.Play(Application.ResourceCache.GetSound(Assets.Sounds.SmallExplosion));
			soundSource.Gain = 0.3f;
			explosionNode.Position = target.Node.WorldPosition;
			explosionNode.SetScale(1.6f);
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D(Assets.Particles.Explosion);
			ScaleBy scaleBy = new ScaleBy(0.3f, 0.1f);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			explosionNode.Remove();
		}
	}
}
