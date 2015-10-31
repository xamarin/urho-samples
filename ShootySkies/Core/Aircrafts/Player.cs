using System;
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
			model.Model = cache.GetModel("Models/Player.mdl");
			var material = cache.GetMaterial("Materials/Player.xml").Clone("");
			model.SetMaterial(material);

			node.SetScale(0.5f);
			node.Position = new Vector3(0f, -6f, 0f);
			node.Rotation = new Quaternion(-40, 0, 0);

			//TODO: rotor should be defined in the model + animation
			rotor = node.CreateChild();
			var rotorModel = rotor.CreateComponent<StaticModel>();
			rotorModel.Model = cache.GetModel("Models/Box.mdl");
			rotorModel.SetMaterial(cache.GetMaterial("Materials/Black.xml"));
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
			particleEmitter.Effect = Application.ResourceCache.GetParticleEffect2D("Particles/PlayerExplosion.pex");
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
			const float moveSpeedY = 2f;
			const float mouseSensitivity = .5f;
			const float touchSensitivity = .1f;
			const float maxY = 3.5f;
			const float maxX = 2.5f;

			var cX = aircraft.Position.X;
			var cY = aircraft.Position.Y;

			float inputX = 0;
			float inputY = 0;

			if (input.NumTouches > 0)
			{
				TouchState state = input.GetTouch(0);
				if (state.Delta.X != 0 || state.Delta.Y != 0)
				{
					inputX = state.Delta.X * touchSensitivity;
					inputY = state.Delta.Y * touchSensitivity;
				}
			}
			else
			{
				var mouseMove = input.MouseMove;
				inputX = mouseSensitivity * mouseMove.X;
				inputY = mouseSensitivity * mouseMove.Y;
			}

			var x = inputX * moveSpeedX * timeStep;
			var y = -inputY * moveSpeedY * timeStep;

			bool outOfBattlefield = (cX + x >= maxX && x > 0) || (cX + x <= -maxX && x < 0) || (cY + y >= maxY && y > 0) || (cY + y<= -maxY && y < 0);

			if (!outOfBattlefield)
			{
				aircraft.Translate(new Vector3(x, y, 0), TransformSpace.World);
			}

			// a small rotation left/right
			if (Math.Abs(x) > 0.01 && !outOfBattlefield)
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
