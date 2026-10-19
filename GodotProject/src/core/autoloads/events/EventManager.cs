using Godot;
using System;

public partial class EventManager : Node
{
    public static EventManager Instance { get; private set; }

    // Paint Signals
    [Signal] public delegate void CoverageChangedEventHandler(float coverage);

    // Player Signals
    [Signal] public delegate void PlayerHealthChangedEventHandler(int current, int max);
    [Signal] public delegate void PlayerDiedEventHandler();

    // Gate Signal
    [Signal] public delegate void GateOpenedEventHandler();












    public override void _Ready()
    {
        Instance = this;
    }
}
