using Godot;
using System;

public partial class Grunt : Node
{
	[Export] public float lungeSpeed;
	[Export] public float splatRadius;
	private Vector2 lungeDir;

	protected override void OnAttackStart()
	{
	}

	protected override Vector2 AttackMovement()
	{
	}

	protected override void OnDeathEffect()
	{
	}
}
