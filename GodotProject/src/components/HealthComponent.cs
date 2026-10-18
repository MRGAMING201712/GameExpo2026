using Godot;
using System;

public partial class HealthComponent : Node
{
    [Export] private int maxHealth = 6; // max health, 1 should equal half a heart so 6 is 3 hearts
    public int currentHealth;

    public override void _Ready()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }
        EventManager.Instance.EmitSignal("PlayerDamaged", damage);
    }

    public void Die()
    {
        // death logic
        EventManager.Instance.EmitSignal("PlayerDied");
        GD.Print("Player has died.");
    }
}
