using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public enum GameState
    {
        MainMenu,
        Playing,
        GateOpen,
        BossFight,
        Won,
        Lost
    }
    public GameState currentState { get;  set; } = GameState.Playing; // change this to main menu when done testing

    // Export variables
    [Export] public float coverageToWin { get; set; } = 0.8f;


    public override void _Ready()
    {
        Instance = this;
        EventManager.Instance.CoverageChanged += OnCoverageChanged;
    }

    public void OnCoverageChanged(float coverage)
    {
        GD.Print(coverage);
        if (currentState == GameState.Playing && coverage >= coverageToWin)
        {
            currentState = GameState.GateOpen;
            EventManager.Instance.EmitSignal("GateOpened");
        }
    }
}
