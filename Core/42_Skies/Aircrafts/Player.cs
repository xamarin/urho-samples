using System;
using System.Linq;

namespace Urho.Samples
{
	public class Player : Aircraft
	{
		public const uint PlayerAircraftCollisionLayer = 2; //specific layer to ignore own bullets

		public Player(Context context) : base(context) {}

		protected override uint CollisionLayer => PlayerAircraftCollisionLayer;

		protected override void Init()
		{
			var cache = Application.Current.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/f16.mdl");

			node.SetScale(0.1f);
			node.Position = new Vector3(0f, -3f, 0f);
			node.Rotation = new Quaternion(10, 0, 0);

			// Load weapons
			node.AddComponent(new MachineGun(Context));
			node.AddComponent(new Missile(Context));

			// Nozzle (engine)
			var nozzle = node.CreateChild("Nozzle");
			nozzle.Position = new Vector3(0, -6, 0);
			var particleEmitter = nozzle.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/sun.pex");

			MoveRandomly();
		}

		async void MoveRandomly()
		{
			while (IsEnabled() && !IsDeleted)
			{
				var moveAction = new MoveBy(0.75f, new Vector3(Sample.NextRandom(-0.4f, 0.4f), Sample.NextRandom(-0.3f, 0.3f), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}
		
		protected override void OnUpdate(UpdateEventArgs args)
		{
			var input = Application.Current.Input;
			var aircraft = Node;
			var timeStep = args.TimeStep;

			var cX = aircraft.Position.X;
			var cY = aircraft.Position.Y;

			const float turnSpeed = 0.5f;
			const float moveSpeed = 5f;
			float maxY = 3.5f, minY = -3.5f;
			float maxX = 2.3f, minX = -2.3f;

			// Forward / Backward
			if (input.GetKeyDown(Key.W) && cY < maxY)
			{
				aircraft.Translate(Vector3.UnitY * moveSpeed * timeStep, TransformSpace.World);
			}
			else if (input.GetKeyDown(Key.S) && cY > minY)
			{
				aircraft.Translate(new Vector3(0.0f, -1.0f, 0.0f) * moveSpeed * timeStep, TransformSpace.World);
				aircraft.Rotate(new Quaternion(-turnSpeed, 0f, 0f), TransformSpace.World);
			}
			else
			{
				// Go back to normal state
				var x = aircraft.Rotation.ToEulerAngles().X;
				if (Math.Abs(x) > 10)
				{
					aircraft.Rotate(new Quaternion(1.5f * turnSpeed * (x > 0 ? -1 : 1), 0, 0), TransformSpace.World);
				}
			}

			// Left / Right
			if (input.GetKeyDown(Key.A) && cX > minX)
			{
				aircraft.Translate(new Vector3(-1.0f, 0.0f, 0.0f) * moveSpeed * timeStep, TransformSpace.World);
				aircraft.Rotate(new Quaternion(0, turnSpeed, 0f), TransformSpace.World);
			}
			else if (input.GetKeyDown(Key.D) && cX < maxX)
			{
				aircraft.Translate(Vector3.UnitX * moveSpeed * timeStep, TransformSpace.World);
				aircraft.Rotate(new Quaternion(0, -turnSpeed, 0f), TransformSpace.World);
			}
			else
			{
				// Go back to normal state
				var y = aircraft.Rotation.ToEulerAngles().Y;
				if (Math.Abs(y) > turnSpeed)
				{
					aircraft.Rotate(new Quaternion(0, 1.5f * turnSpeed * (y > 0 ? 1 : -1), 0), TransformSpace.World);
				}
			}

			// Fire
			if (input.GetKeyDown(Key.Space))
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					weapon.FireAsync(byPlayer: true);
				}
			}
		}
	}
}
