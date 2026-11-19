using Godot;
using System;

public partial class Eraser : Enemy
{
	[Export] public float walkEraseRadius = 10f;
	[Export] public float slamEraseRadius = 30f;
	private Vector2 target;
	private bool hasTarget;
	private double retargetTimer;

	protected override void Think()
	{
		MoveToward(target, speed);
		FaceToward(target);
		ChasePlayer(speed);
		PaintManager.Instance.Erase(GlobalPosition, walkEraseRadius);
	}

	protected override void OnAttackStart()
	{
		PaintManager.Instance.Erase(GlobalPosition, slamEraseRadius);
	}
}
