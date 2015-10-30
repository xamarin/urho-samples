using System;
using System.Linq;
using Urho;

namespace ShootySkies
{
	public class Player : Aircraft
	{
		public const uint PlayerAircraftCollisionLayer = 2; //specific layer to ignore own bullets

		public Player(Context context) : base(context) {}

		protected override uint CollisionLayer => PlayerAircraftCollisionLayer;

		public override int MaxHealth => 100;

		protected override void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Player.mdl");
			var material = cache.GetMaterial("Materials/Player.xml").Clone("");
			model.SetMaterial(material);

			node.SetScale(0.5f);
			node.Position = new Vector3(0f, -3f, 0f);
			node.Rotation = new Quaternion(-40, 0, 0);

			// Load weapons
			node.AddComponent(new MachineGun(Context));
			node.AddComponent(new Missile(Context));

			MoveRandomly();
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

			var cX = aircraft.Position.X;
			var cY = aircraft.Position.Y;

			const float turnSpeed = 1f;
			const float moveSpeedX = 3f;
			const float moveSpeedY = 2f;

			float maxY = 3.5f;
			float maxX = 2.5f;

			const float mouseSensitivity = .5f;
			const float touchSensitivity = .1f;


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
