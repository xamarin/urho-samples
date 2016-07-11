using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urho;
using Urho.Actions;

namespace SamplyGame.Aircrafts.Enemies
{
	/// <summary>
	/// A component to rule all enemies
	/// </summary>
	public class Enemies : Component
	{
		readonly Player player;
		readonly List<Enemy> enemies;

		public Enemies(Player player)
		{
			enemies = new List<Enemy>();
			this.player = player;
		}

		public void KillAll()
		{
			foreach (var enemy in enemies.ToArray())
			{
				enemy.Explode();
			}
		}

		public async void StartSpawning()
		{
			int count = 3;
			while (player.IsAlive)
			{
				await SpawnBats(count: count++, pause: 1f);
				await SpawnTwoMonitors();
			}
		}

		Task SpawnTwoMonitors()
		{
			return Task.WhenAll(
				SpawnEnemy(() => new EnemyEvilMonitor(true), 1), 
				SpawnEnemy(() => new EnemyEvilMonitor(false), 1));
		}

		async Task SpawnBats(int count, float pause)
		{
			var tasks = new List<Task>();
			for (int i = 1; i < count + 1 && player.IsAlive; i++)
			{
				if (i %  3 == 0) 
					tasks.Add(SpawnEnemy(() => new EnemySkull(), 3));
				else
					tasks.Add(SpawnEnemy(() => new EnemyXamonkey(), 3));

				await Node.RunActionsAsync(new DelayTime(pause));
			}
			await Task.WhenAll(tasks);
		}

		async Task SpawnEnemy(Func<Enemy> enemyFactory, int times)
		{
			for (int i = 0; i < times && player.IsAlive; i++)
			{
				var enemyNode = Node.CreateChild(nameof(Aircraft));
				var enemy = enemyFactory();
				enemyNode.AddComponent(enemy);
				enemies.Add(enemy);
				await enemy.Play();
				enemies.Remove(enemy);
				enemyNode.RemoveAllActions();
				enemyNode.Remove();
			}
		}
	}
}
