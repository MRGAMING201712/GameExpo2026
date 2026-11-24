using Godot;
using System;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene[] enemyScenes = {};
	[Export] public bool nonBuilding = true;
	[Export] public float minDistance = 120f;
	[Export] public float spawnRadius = 30f;

	private float spawnTime;

	public void _on_timer_timeout()
	{
		if(!FindSpawnPoint(out Vector2 pos))
		{
			return;
		}
		int randomIndex = GD.RandRange(0, enemyScenes.Length - 1);
		Node2D enemy = enemyScenes[randomIndex].Instantiate<Node2D>();
		AddChild(enemy);
		enemy.GlobalPosition = pos;
	}

	private bool FindSpawnPoint(out Vector2 pos)
	{
		Rect2 area = new Rect2(GlobalPosition - new Vector2(spawnRadius, spawnRadius), new Vector2(spawnRadius * 2f, spawnRadius * 2f));
		Player player = MainGame.Instance?.player;
		for(int i = 0; i < 30; i++)
		{
			if (!PaintManager.Instance.FindUnpaintedCell(area, out Vector2 candidate, 1))
			{
				continue;
			}
			if(candidate.DistanceTo(player.GlobalPosition) < minDistance)
			{
				continue;
			}

			pos = candidate;
			return true;
		}
		pos = Vector2.Zero;
		return false;
	}

}
