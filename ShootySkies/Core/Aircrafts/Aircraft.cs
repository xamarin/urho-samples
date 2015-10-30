using System.Threading;
using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public abstract class Aircraft : Component
	{
		TaskCompletionSource<bool> liveTask;
		bool isAlive;

		protected Aircraft(Context context) : base(context) {}

		public Task Play()
		{
			liveTask = new TaskCompletionSource<bool>();
			Health = MaxHealth;
			Application.Update += OnUpdate;
			var node = Node;

			// Define physics
			var body = node.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.SetKinematic(true);
			body.CollisionMask = CollisionLayer;
			CollisionShape shape = node.CreateComponent<CollisionShape>();
			shape.SetBox(Vector3.One * 1.1f, Vector3.Zero, Quaternion.Identity);
			isAlive = true;
			Init();
			return liveTask.Task;
		}

		public async virtual Task Explode()
		{
			isAlive = false;
			var cache = Application.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.SetScale(1f);
			explosionNode.Position = this.Node.WorldPosition;
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/Explosion.pex");
			ScaleBy scaleBy = new ScaleBy(0.7f, 0f);
			Node.RemoveAllActions();
			Node.SetEnabled(false);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			liveTask.TrySetResult(true);
			explosionNode.Remove();
		}

		public void Hit()
		{
			// blink with white color:
			var specColorAnimation = new ValueAnimation(Context);
			specColorAnimation.SetKeyFrame(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			specColorAnimation.SetKeyFrame(0.2f, new Color(0.1f, 0.1f, 0.1f, 16.0f));
			Node.GetComponent<StaticModel>().GetMaterial(0)?.SetShaderParameterAnimation("MatSpecColor", specColorAnimation, WrapMode.Once, 1.0f);
		}

		public int Health { get; set; }

		public virtual int MaxHealth => 30;

		public bool IsAlive => isAlive && IsEnabled() && !IsDeleted;

		protected virtual void Init() {}

		protected virtual uint CollisionLayer => 1;

		protected override void OnDeleted()
		{
			Application.Update -= OnUpdate;
		}

		protected virtual void OnUpdate(UpdateEventArgs args) {}
	}
}
