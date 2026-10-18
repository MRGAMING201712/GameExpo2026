using Godot;
using System;

public partial class EventManager : Node
{
    public static EventManager Instance { get; private set; }
    
    // Player Signals
    [Signal] public delegate void PlayerDamagedEventHandler(int damage);
    [Signal] public delegate void PlayerDiedEventHandler();










    public override void _Ready()
    {
        Instance = this;
    }
}
