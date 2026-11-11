using Godot;
using System;

public partial class Character : Node
{
	[Export] public HealthComponent healthComponent;
    protected bool facingLeft;

	public virtual void TakeDamage(int amount, Vector2 hitFrom = default)
    {
	}

	protected virtual void Die()
    {
	}

	protected override void OnAttackHit()
    {
	}

	protected override void OnDeathEffect()
    {
	}
}
