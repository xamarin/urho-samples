using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;
using Urho.Shapes;

namespace SamplyGame
{
	public class MassCross : Weapon
	{
		public override TimeSpan ReloadDuration => TimeSpan.FromSeconds(2f);

		public override int Damage => 10;

		protected override Task OnFire(bool byPlayer)
		{
			const int joysticksCount = 12;
			const float length = 10f;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < joysticksCount; i++)
			{
				var angle = MathHelper.DegreesToRadians(360 / joysticksCount * i); //angle per joystick (in radians)
				//x^2 + y^2 = length^2 (Equation of Circle):
				var x = (float) Math.Cos(angle) * length;
				var y = (float) Math.Sin(angle) * length;
				tasks.Add(Fire(new Vector3(x, y, 0), byPlayer));
			}
			return Task.WhenAll(tasks);
		}

		async Task Fire(Vector3 direction, bool byPlayer)
		{
			var bulletNode = CreateRigidBullet(byPlayer);
			bulletNode.Rotation = new Quaternion(-50, 0, 0);
			bulletNode.SetScale(0.1f);

			var color = Color.White;
			var nodeX = bulletNode.CreateChild();
			nodeX.Scale = new Vector3(5, 1, 1);
			var boxX = nodeX.CreateComponent<Box>();
			boxX.Color = color;

			var nodeZ = bulletNode.CreateChild();
			nodeZ.Scale = new Vector3(1, 1, 5);
			var boxZ = nodeZ.CreateComponent<Box>();
			boxZ.Color = color;

			var nodeY = bulletNode.CreateChild();
			nodeY.Scale = new Vector3(1, 5, 1);
			var boxY = nodeY.CreateComponent<Box>();
			boxY.Color = color;

			bulletNode.RunActions(new RepeatForever(new RotateBy(0.2f,Randoms.Next(30, 60), Randoms.Next(30, 60), 0f)));

			await bulletNode.RunActionsAsync(
				new MoveBy(5f, direction),
				new CallFunc(() => bulletNode.SetScale(0f))); //collapse);

			bulletNode.Remove();
		}
	}
}
