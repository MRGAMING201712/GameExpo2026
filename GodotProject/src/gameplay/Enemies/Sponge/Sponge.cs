using Godot;
using System;

public partial class Sponge : Node
{
	[Export] public float soakRadius;
    [Export] public float slamSoakRadius;
    [Export] public float growthPerCell;
    [Export] public float maxScale;
    [Export] public float maxSpeedMultiplier;
    private float absorbed;
    private float baseSpeed;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	private void Soak(float radius)
    {
	}

	protected override void Think(double delta)
    {
	}
}
