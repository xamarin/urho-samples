using System.Threading.Tasks;

namespace Urho.Samples
{
	public abstract class Aircraft : Component
	{
		TaskCompletionSource<bool> liveTask;

		public Aircraft(Context context) : base(context) {}

		public Task Play(int health)
		{
			liveTask = new TaskCompletionSource<bool>();
			Health = health;
			Application.Current.Update += OnUpdate;
			var node = Node;

			// Define physics
			var body = node.CreateComponent<RigidBody>();
			body.Mass = 1;
			body.SetKinematic(true);
			body.CollisionMask = CollisionLayer;
			CollisionShape shape = node.CreateComponent<CollisionShape>();
			shape.SetBox(new Vector3(6, 6, 6), Vector3.Zero, Quaternion.Identity);

			Init();

			return liveTask.Task;
		}

		public virtual async Task Explode()
		{
			var cache = Application.Current.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.SetScale(1.8f);
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

		public int Health { get; set; }

		protected virtual void Init() {}

		protected virtual uint CollisionLayer => 1;

		protected override void OnDeleted()
		{
			Application.Current.Update -= OnUpdate;
		}

		protected virtual void OnUpdate(UpdateEventArgs args) {}
	}
}
