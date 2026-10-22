using Godot;
using System;

public partial class Gate : Node2D
{
    [Export] public AnimatedSprite2D gateSprite { get; set; }
    [Export] public CollisionShape2D gateCollision { get; set; }
    [Export] public Area2D gateTrigger {get; set;}

    private bool bIsOpen = false;
    private bool bUsed = false;

    public override void _Ready()
    {
        EventManager.Instance.GateOpened += OnGateOpened;
        gateTrigger.BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        EventManager.Instance.GateOpened -= OnGateOpened;
    }

    public void OnGateOpened()
    {
        gateSprite.Animation = "opened";
        gateCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        bIsOpen = true;
    }

    public void OnBodyEntered(Node2D body)
    {
        if (!bIsOpen || bUsed || body is not Player)
        {
          return;  
        } 
        bUsed = true;
        Callable.From(() => EventManager.Instance.EmitSignal(EventManager.SignalName.TransitionLevels)).CallDeferred();
    }
}