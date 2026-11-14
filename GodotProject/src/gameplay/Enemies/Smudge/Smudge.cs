using Godot;
using System;

public partial class Smudge : Enemy
{
	[Export] public float lungeSpeed;
	[Export] public float splatRadius;
	private Vector2 lungeDir;

	protected override void OnAttackStart()
	{
		base.OnAttackStart();
		lungeDir = GlobalPosition.DirectionTo(player.GlobalPosition);
	}

	protected override Vector2 AttackMovement()
	{
		return lungeDir * lungeSpeed;
	}

	protected override void OnDeathEffect()
	{
		base.OnDeathEffect();
	}
}
