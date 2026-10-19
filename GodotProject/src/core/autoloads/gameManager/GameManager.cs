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
    public GameState currentState { get;  set; } = GameState.MainMenu ; // change this to main menu when done testing

    // Export variables
    [Export] public float coverageToWin { get; set; } = 0.8f;
    // Level info
    [Export] public PackedScene mainMenuScreen;
    [Export] public PackedScene mainGameScene;
    [Export] public PackedScene [] levelScenes; // not including boss level
    [Export] public PackedScene bossLevelScene;
    [Export] public PackedScene gameOverScene;
    public int kills {get; set;}
    public int zoneIndex {get; set;}
    public int enemiesAlive {get; set;} // unsure if we will have ui for enemies alive, but if we do this is the counter
    public Player player = null;


    public override void _Ready()
    {
        Instance = this;
        EventManager.Instance.CoverageChanged += OnCoverageChanged;
        EventManager.Instance.TransitionLevels += OnTransitionLevels;
    }

    public void MainGameReady()
    {
        StartRun();
        GoToLevel(levelScenes[zoneIndex], GameState.Playing);
    }

    public void StartRun()
    {
        kills = 0;
        zoneIndex = 0;
        enemiesAlive = 0;
        GetTree().Paused = false;
    }

    public async void GoToLevel(PackedScene newLevel, GameState newState)
    {
        currentState = GameState.MainMenu;
        enemiesAlive = 0;

        await MainGame.Instance.LoadLevel(newLevel, player);
        currentState = newState;
        EventManager.Instance.EmitSignal(EventManager.SignalName.ZoneStarted, zoneIndex);
    }

    public void OnTransitionLevels()
    {
        if(zoneIndex + 1 >= levelScenes.Length)
        {
            GoToLevel(bossLevelScene, GameState.BossFight);
            zoneIndex ++;
            return;
        }
        zoneIndex ++;
        GoToLevel(levelScenes[zoneIndex], GameState.Playing);

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
