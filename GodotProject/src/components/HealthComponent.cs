using Godot;
using System;

public partial class HealthComponent : Node
{
    // Signals, export the healComponent on the player and enemy scripts so they can listen
    [Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();

    [Export] private int maxHealth = 6; // max health, 1 should equal half a heart so 6 is 3 hearts
    public int currentHealth;

    public override void _Ready()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if(damage <= 0)
        {
           return;  
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        EmitSignal("HealthChanged", currentHealth, maxHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            EmitSignal("Died");
        }
    }

}
