using System.Linq;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Shapes;
using Urho.Urho2D;

namespace SamplyGame
{
	public class Player : Aircraft
	{
		Node rotor;

		protected override CollisionLayers CollisionLayer => CollisionLayers.Player;

		protected override Vector3 CollisionShapeSize => new Vector3(3.1f, 1.2f, 1.2f); // extend default shape to get collisions by wings too

		public override int MaxHealth => 40;

		protected override async void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Player1);
			var material = cache.GetMaterial(Assets.Materials.Player1D).Clone("");
			model.SetMaterial(material);
			
			node.SetScale(0.35f);
			node.Position = new Vector3(0f, -6f, 0f);
			node.Rotation = new Quaternion(0, 0, 180);

			//TODO: rotor should be defined in the model + animation
			rotor = node.CreateChild();
			var rotorModel = rotor.CreateComponent<Box>();
			rotorModel.Color = Color.White;
			rotor.Scale = new Vector3(0.1f, 1.5f, 0.1f);
			rotor.Position = new Vector3(0, -0.15f, -1.5f);
			rotor.RunActions(new RepeatForever(new RotateBy(1f, 0, 0, 360f * 4))); //RPM

			// Load weapons
			node.AddComponent(new MachineGun());
			node.AddComponent(new Missile());

			await node.RunActionsAsync(new EaseOut(new MoveBy(0.5f, new Vector3(0, 3, 0)), 2));
			MoveRandomly();

			// Do an alleron roll each 5 seconds
			Node.RunActions(new RepeatForever(new DelayTime(5),
					new EaseBackInOut(new RotateBy(1f, 0f, 0f, 360))));
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
		
		protected override void OnUpdate(float timeStep)
		{
			if (!IsAlive)
				return;

			var input = Application.Current.Input;
			var aircraft = Node;

			int positionX = 0, positionY = 0;
			bool hasInput = false;
			if (input.NumTouches > 0)
			{
				// move with touches:
				TouchState state = input.GetTouch(0);
				var touchPosition = state.Position;
				positionX = touchPosition.X;
				positionY = touchPosition.Y;
				hasInput = true;
			}
			else if (input.GetMouseButtonDown(MouseButton.Left))
			{
				// move with mouse:
				var mousePos = input.MousePosition;
				positionX = mousePos.X;
				positionY = mousePos.Y;
				hasInput = true;
			}

			if (hasInput)
			{
				Vector3 destWorldPos = ((SamplyGame)Application).Viewport.ScreenToWorldPoint(positionX, positionY, 10);
				destWorldPos.Z = 0;
				aircraft.Translate(destWorldPos - aircraft.WorldPosition, TransformSpace.World);
				foreach (var weapon in Node.Components.OfType<Weapon>())
				{
					weapon.FireAsync(true);
				}
			}

			aircraft.LookAt(new Vector3(0, aircraft.WorldPosition.Y + 10, 10), new Vector3(0, 1, -1), TransformSpace.World);
			aircraft.Rotate(new Quaternion(0, 180, 0), TransformSpace.Local);
		}
	}
}
