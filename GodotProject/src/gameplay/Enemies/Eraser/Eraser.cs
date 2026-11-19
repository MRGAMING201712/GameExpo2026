using Godot;
using System;

public partial class Eraser : Enemy
{
	[Export] public float walkEraseRadius = 10f;
	[Export] public float slamEraseRadius = 30f;
	private Vector2 target;
	//private bool hasTarget;
	//private double retargetTimer;
	private Vector2 lungeDir;

	protected override void Think()
	{
		
		ChasePlayer(speed);
		PaintManager.Instance.Erase(GlobalPosition, walkEraseRadius);
	}

	protected override void OnAttackStart()
	{
		base.OnAttackStart();
		target = GlobalPosition.DirectionTo(player.GlobalPosition);
		PaintManager.Instance.Erase(GlobalPosition, slamEraseRadius);
	}

	protected override Vector2 AttackMovement()
	{
		return target * speed;
	}
}
