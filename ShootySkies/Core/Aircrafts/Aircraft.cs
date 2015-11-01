using System.Linq;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public abstract class Aircraft : Component
	{
		TaskCompletionSource<bool> liveTask;
		bool isExploding;

		protected Aircraft(Context context) : base(context) {}

		public int Health { get; set; }
		public virtual int MaxHealth => 30;
		public bool IsAlive => Health > 0 && IsEnabled() && !IsDeleted;

		public Task Play()
		{
			liveTask = new TaskCompletionSource<bool>();
			Health = MaxHealth;
			Application.SceneUpdate += OnUpdate;
			var node = Node;

			// Define physics
			var body = node.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.SetKinematic(true);
			body.CollisionMask = CollisionLayer;
			CollisionShape shape = node.CreateComponent<CollisionShape>();
			shape.SetBox(new Vector3(3, 1.2f, 1.2f), Vector3.Zero, Quaternion.Identity);
			Init();
			node.SubscribeToNodeCollisionStart(OnCollided);
			return liveTask.Task;
		}

		public async Task Explode()
		{
			if (isExploding) //since the method is async we need to protect the aircraft from being exploded twice
				return;
			isExploding = true;
			Health = 0;
			var explosionNode = Scene.CreateChild();
			explosionNode.Position = Node.WorldPosition;
			OnExplode(explosionNode);
			ScaleBy scaleBy = new ScaleBy(0.7f, 0f);
			Node.RemoveAllActions();
			Node.SetEnabled(false);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			liveTask.TrySetResult(true);
			explosionNode.Remove();
		}

		void OnCollided(NodeCollisionStartEventArgs args)
		{
			var bulletNode = args.OtherNode;
			if (IsAlive && bulletNode.Name != null && bulletNode.Name.StartsWith(nameof(Weapon)) && args.Body.Node == Node)
			{
				// TODO: fix #69 (GetComponent<T> for custom components)
				var weapon = ((WeaponReferenceComponent)bulletNode.Components.First(c => c is WeaponReferenceComponent)).Weapon;
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
			// blink with white color:
			var specColorAnimation = new ValueAnimation(Context);
			specColorAnimation.SetKeyFrame(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			specColorAnimation.SetKeyFrame(0.1f, new Color(0.1f, 0.1f, 0.1f, 16.0f));
			Node.GetComponent<StaticModel>().GetMaterial(0)?.SetShaderParameterAnimation("MatSpecColor", specColorAnimation, WrapMode.Once, 1.0f);
			await Node.RunActionsAsync(new DelayTime(1f));
		}

		protected virtual void OnExplode(Node explodeNode)
		{
			explodeNode.SetScale(1.75f);
			var particleEmitter = explodeNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = Application.ResourceCache.GetParticleEffect2D(Assets.Particles.Explosion);
		}

		protected virtual void Init() {}

		protected virtual uint CollisionLayer => 1;

		protected override void OnDeleted()
		{
			Application.SceneUpdate -= OnUpdate;
		}

		protected virtual void OnUpdate(SceneUpdateEventArgs sceneUpdateEventArgs) {}
	}
}
