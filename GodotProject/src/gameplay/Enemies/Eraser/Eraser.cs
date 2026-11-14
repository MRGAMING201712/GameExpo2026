using Godot;
using System;

public partial class Eraser : Node
{
	[Export] public float walkEraseRadius;
	[Export] public float slamEraseRadius;
	private Vector2 target;
	private bool hasTarget;
	private double retargetTimer;

	//protected override void Think(double delta)
	//{
	//}

	//protected override void OnAttackHit()
	//{
	//}
}
