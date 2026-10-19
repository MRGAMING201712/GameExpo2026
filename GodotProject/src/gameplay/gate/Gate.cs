using Godot;
using System;

public partial class Gate : Node2D
{
    [Export] public AnimatedSprite2D gateSprite { get; set; }
    [Export] public CollisionShape2D gateCollision { get; set; }
    public override void _Ready()
    {
        EventManager.Instance.GateOpened += OnGateOpened;
    }

    public void OnGateOpened()
    {
        gateSprite.Animation = "opened";
        gateCollision.Disabled = true;
    }
}
