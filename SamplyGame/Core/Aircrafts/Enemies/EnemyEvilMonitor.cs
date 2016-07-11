using Urho;
using Urho.Actions;

namespace SamplyGame
{
	public class EnemyEvilMonitor : Enemy
	{
		readonly bool fromLeftSide;
		
		public EnemyEvilMonitor(bool fromLeftSide)
		{
			this.fromLeftSide = fromLeftSide;
		}

		public override int MaxHealth => 80;


		protected override async void Init()
		{
			var cache = Application.ResourceCache;
			var node = Node;
			var model = node.CreateComponent<StaticModel>();
			model.Model = cache.GetModel(Assets.Models.Enemy3);
			model.SetMaterial(cache.GetMaterial(Assets.Materials.Enemy3).Clone(""));
			node.SetScale(1f);

			// load weapons:
			node.AddComponent(new SmallPlate());

			var direction = fromLeftSide ? -1 : 1;
			node.Position = new Vector3(3 * direction, 3, 0);
			await Node.RunActionsAsync(new MoveTo(0.5f, new Vector3(1.6f * direction, 2.5f, 0)));
			MoveRandomly(minX: -0.5f, maxX: 0.5f, minY: -0.5f, maxY: 0.5f, duration: 1f);
			StartShooting();
		}
	}
}