using System;
using System.Diagnostics;
using System.Linq;
using Urho;

namespace ShootySkies
{
	public class Player : Aircraft
	{
		Node rotor;

		public const uint PlayerAircraftCollisionLayer = 2; //specific layer to ignore own bullets

		public Player(Context context) : base(context) {}

		protected override uint CollisionLayer => PlayerAircraftCollisionLayer;

		public override int MaxHealth => 100;

		protected override async void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Player);
			var material = cache.GetMaterial(Assets.Materials.Player).Clone("");
			model.SetMaterial(material);

			node.SetScale(0.45f);
			node.Position = new Vector3(0f, -6f, 0f);
			node.Rotation = new Quaternion(-40, 0, 0);

			//TODO: rotor should be defined in the model + animation
			rotor = node.CreateChild();
			var rotorModel = rotor.CreateComponent<StaticModel>();
			rotorModel.Model = cache.GetModel(Assets.Models.Box);
			rotorModel.SetMaterial(cache.GetMaterial(Assets.Materials.Black));
			rotor.Scale = new Vector3(0.1f, 1.4f, 0.1f);
			rotor.Rotation = new Quaternion(0, 0, 0);
			rotor.Position = new Vector3(0, -0.15f, 1.2f);
			rotor.RunActionsAsync(new RepeatForever(new RotateBy(1f, 0, 0, 360f * 4))); //RPM

			// Load weapons
			node.AddComponent(new MachineGun(Context));
			node.AddComponent(new Missile(Context));

			await node.RunActionsAsync(new EaseOut(new MoveBy(0.5f, new Vector3(0, 3, 0)), 2));
			MoveRandomly();
		}

		protected override void OnExplode(Node explodeNode)
		{
			rotor.RemoveAllActions();
			rotor.Remove();
			var particleEmitter = explodeNode.CreateComponent<ParticleEmitter2D>();
			explodeNode.SetScale(1.5f);
			particleEmitter.Effect = Application.ResourceCache.GetParticleEffect2D(Assets.Particles.PlayerExplosion);
		}

		async void MoveRandomly()
		{
			while (IsAlive)
			{
				var moveAction = new MoveBy(0.75f, new Vector3(RandomHelper.NextRandom(-0.4f, 0.4f), RandomHelper.NextRandom(-0.3f, 0.3f), 0));
				await Node.RunActionsAsync(moveAction, moveAction.Reverse());
			}
		}

		protected override void OnUpdate(UpdateEventArgs args)
		{
			if (!IsAlive)
				return;

			var input = Application.Current.Input;
			var aircraft = Node;
			var timeStep = args.TimeStep;

			const float turnSpeed = 1f;
			const float moveSpeedX = 3f;
			const float mouseSensitivity = .5f;
			const float touchSensitivity = .1f;

			float inputX = 0;
			IntVector2 touchPosition;

			if (input.NumTouches > 0)
			{
				TouchState state = input.GetTouch(0);
				if (state.Delta.X != 0 || state.Delta.Y != 0)
				{
					inputX = state.Delta.X * touchSensitivity;
				}
				touchPosition = state.Position;

				var srcPos = aircraft.WorldPosition;
				Vector3 destWorldPos = ((ShootySkiesGame)Application).Viewport.ScreenToWorldPoint(touchPosition.X, touchPosition.Y, 10);

				var delta = (destWorldPos - srcPos);
				aircraft.Translate(delta, TransformSpace.World);
			}
			else
			{
				var mouseMove = input.MouseMove;
				inputX = mouseSensitivity * mouseMove.X;
			}

			var x = inputX * moveSpeedX * timeStep;

			// a small rotation left/right
			if (Math.Abs(x) > 0.01)
			{
				aircraft.Rotate(new Quaternion(0, turnSpeed * (x > 0 ? -1 : 1), 0f), TransformSpace.World);
			}
			else
			{
				// Go back to normal state
				var yAngle = aircraft.Rotation.ToEulerAngles().Y;
				if (Math.Abs(yAngle) > turnSpeed)
				{
					aircraft.Rotate(new Quaternion(0, 1.5f * turnSpeed * (yAngle > 0 ? 1 : -1), 0), TransformSpace.World);
				}
			}

			// Fire 
			if (input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches > 0)
			{
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					weapon.FireAsync(byPlayer: true);
				}
			}
		}
	}
}
