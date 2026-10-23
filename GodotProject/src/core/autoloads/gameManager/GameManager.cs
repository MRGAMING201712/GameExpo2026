using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    public enum GameState
    {
        MainMenu,
        Transitioning,
        Playing,
        GateOpen,
        BossFight,
        Won,
        Lost
    }
    public GameState currentState { get;  set; } = GameState.MainMenu ;

    // True only when the player should move, swing and switch colors
    public bool CanControlPlayer => currentState is GameState.Playing or GameState.GateOpen or GameState.BossFight;

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

    private bool bIsTransitioning = false;


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
        bIsTransitioning = false;
        GetTree().Paused = false;
    }

    public async void GoToLevel(PackedScene newLevel, GameState newState)
    {
        if (bIsTransitioning)
        {
          return;  
        } 
        bIsTransitioning = true;

        currentState = GameState.Transitioning;
        enemiesAlive = 0;

        LevelTransition transition = MainGame.Instance.transitionRoot;

        await transition.FadeToBlack();

        MainGame.Instance.LoadLevel(newLevel);

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree().CreateTimer(transition.holdDuration), SceneTreeTimer.SignalName.Timeout);

        EventManager.Instance.EmitSignal(EventManager.SignalName.ZoneStarted, zoneIndex);

        await transition.FadeFromBlack();

        currentState = newState;
        bIsTransitioning = false;
    }

    public void OnTransitionLevels()
    {
        if (bIsTransitioning)
        {
           return; 
        } 

        zoneIndex++;
        if (zoneIndex >= levelScenes.Length)
        {
           GoToLevel(bossLevelScene, GameState.BossFight); 
        }
            
        else
        {
            GoToLevel(levelScenes[zoneIndex], GameState.Playing);
        }
            
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