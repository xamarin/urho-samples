using System.Linq;
using System.Threading.Tasks;
using Urho;
using Urho.Physics;
using Urho.Audio;
using Urho.Actions;
using Urho.Shapes;
using Urho.Urho2D;

namespace SamplyGame
{
	/// <summary>
	/// A base class for all aircrafts including the player and enemies
	/// </summary>
	public abstract class Aircraft : Component
	{
		TaskCompletionSource<bool> liveTask;

		protected Aircraft()
		{
			ReceiveSceneUpdates = true;
		}

		/// <summary>
		/// Current health (less or equal to MaxHealth)
		/// </summary>
		public int Health { get; set; }

		/// <summary>
		/// Max health value 
		/// </summary>
		public virtual int MaxHealth => 30;
		
		/// <summary>
		/// Is aircraft alive
		/// </summary>
		public bool IsAlive => Health > 0 && Enabled && !IsDeleted;

		/// <summary>
		/// Spawn the aircraft and wait until it's exploded
		/// </summary>
		public Task Play()
		{
			liveTask = new TaskCompletionSource<bool>();
			Health = MaxHealth;
			var node = Node;

			// Define physics for handling collisions
			var body = node.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.Kinematic = true;
			body.CollisionMask = (uint)CollisionLayer;
			CollisionShape shape = node.CreateComponent<CollisionShape>();
			shape.SetBox(CollisionShapeSize, Vector3.Zero, Quaternion.Identity);

			Init();
			node.SubscribeToNodeCollisionStart(OnCollided);
			return liveTask.Task;
		}

		/// <summary>
		/// Explode the aircraft with animation
		/// </summary>
		public async Task Explode()
		{
			Health = 0;
			//create a special independent node in the scene for explosion
			var explosionNode = Scene.CreateChild();
			SoundSource soundSource = explosionNode.CreateComponent<SoundSource>();
			soundSource.Play(Application.ResourceCache.GetSound(Assets.Sounds.BigExplosion));
			soundSource.Gain = 0.5f;

			explosionNode.Position = Node.WorldPosition;
			OnExplode(explosionNode);
			var scaleAction = new ScaleTo(1f, 0f);
			Node.RemoveAllActions();
			Node.Enabled = false;
			await explosionNode.RunActionsAsync(scaleAction, new DelayTime(1f));
			liveTask.TrySetResult(true);
			explosionNode.Remove();
		}

		void OnCollided(NodeCollisionStartEventArgs args)
		{
			var bulletNode = args.OtherNode;
			if (IsAlive && bulletNode.Name != null && bulletNode.Name.StartsWith(nameof(Weapon)) && args.Body.Node == Node)
			{
				var weapon = bulletNode.GetComponent<WeaponReferenceComponent>().Weapon;
				Health -= weapon.Damage;
				var killed = Health <= 0;
				if (killed)
				{
					Explode();
				}
				else if (weapon.Damage > 0)
				{
					Hit();
				}
				weapon.OnHit(target: this, killed: killed, bulletNode: bulletNode);
			}
		}

		async void Hit()
		{
			var material = Node.GetComponent<StaticModel>().GetMaterial(0);
			if (material == null)
				return;
			//NOTE: the material should not be cached (Clone() should be called) or all object with it will be blinking
			material.SetShaderParameter("MatSpecColor", new Color(0, 0, 0, 0));
			var specColorAnimation = new ValueAnimation();
			specColorAnimation.SetKeyFrame(0.0f, new Color(1.0f, 1.0f, 1.0f, 0.5f));
			specColorAnimation.SetKeyFrame(0.1f, new Color(0, 0, 0, 0));
			material.SetShaderParameterAnimation("MatSpecColor", specColorAnimation, WrapMode.Once, 1.0f);
			await Node.RunActionsAsync(new DelayTime(1f));
		}

		protected virtual void OnExplode(Node explodeNode)
		{
			explodeNode.SetScale(2f);
			var particleEmitter = explodeNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = Application.ResourceCache.GetParticleEffect2D(Assets.Particles.Explosion);
		}

		protected virtual void Init() {}

		protected virtual CollisionLayers CollisionLayer => CollisionLayers.Enemy;

		protected virtual Vector3 CollisionShapeSize => new Vector3(1.2f, 1.2f, 1.2f);
	}

	public enum CollisionLayers : uint
	{
		Player = 2,
		Enemy = 4
	}
}
