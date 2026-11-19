using Godot;
using System;

public partial class Character : CharacterBody2D
{
	[Export] public HealthComponent healthComponent;
	protected bool facingLeft;

	public override void _Ready()
	{
		healthComponent.Died += Die;
	}

	public virtual void TakeDamage(int amount, Vector2 hitFrom = default)
	{
		healthComponent.TakeDamage(amount);
		GD.Print($"Character took {amount} damage. Current health: {healthComponent.currentHealth}");
	}

	protected virtual void Die()
	{
		QueueFree();
	}

	
}
