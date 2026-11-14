using Godot;
using System;

public partial class Critic : Node
{
	[Export] public float wipeInterval;
	[Export] public float telegraphTime;
	private double wipeTimer;
	private double telegraphTimer;
	private bool wiping;  
	// Called when the node enters the scene tree for the first time.
	//public override void _Ready()
	//{
	//}

	//protected override void Tick(double delta)
	//{
	//}

	//protected override void OnDeathEffect()
	//{
	//}


}
