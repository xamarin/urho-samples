using System.Threading.Tasks;
using Urho;

namespace ShootySkies
{
	public abstract class Aircraft : Component
	{
		TaskCompletionSource<bool> liveTask;
		bool isAlive;

		protected Aircraft(Context context) : base(context) {}

		public Task Play(int health)
		{
			isAlive = true;
			liveTask = new TaskCompletionSource<bool>();
			Health = health;
			Application.Update += OnUpdate;
			var node = Node;

			// Define physics
			var body = node.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.SetKinematic(true);
			body.CollisionMask = CollisionLayer;
			CollisionShape shape = node.CreateComponent<CollisionShape>();
			shape.SetBox(new Vector3(2, 2, 2), Vector3.Zero, Quaternion.Identity);

			Init();

			return liveTask.Task;
		}

		public async Task Explode()
		{
			isAlive = false;
			var cache = Application.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.SetScale(1f);
			explosionNode.Position = this.Node.WorldPosition;
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/sun2.pex");
			ScaleBy scaleBy = new ScaleBy(0.7f, 0f);
			Node.RemoveAllActions();
			Node.SetEnabled(false);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			liveTask.TrySetResult(true);
			explosionNode.Remove();
		}

		public void Hit()
		{
			var specColorAnimation = new ValueAnimation(Context);
			specColorAnimation.SetKeyFrame(0.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f));
			specColorAnimation.SetKeyFrame(0.2f, new Color(0.1f, 0.1f, 0.1f, 16.0f));
			Node.GetComponent<StaticModel>().GetMaterial(0).SetShaderParameterAnimation("MatSpecColor", specColorAnimation, WrapMode.Once, 1.0f);
		}

		public int Health { get; set; }

		protected bool IsAlive => isAlive && IsEnabled() && !IsDeleted;

		protected virtual void Init() {}

		protected virtual uint CollisionLayer => 1;

		protected override void OnDeleted()
		{
			Application.Update -= OnUpdate;
		}

		protected virtual void OnUpdate(UpdateEventArgs args) {}
	}
}
