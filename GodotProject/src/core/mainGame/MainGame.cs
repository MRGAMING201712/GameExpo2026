using Godot;
using System;
using System.Threading.Tasks;

public partial class MainGame : Node
{
    public static MainGame Instance { get; private set; }
    [Export] public Node2D levelRoot { get; set; }
    [Export] public Node2D groundVFXRoot { get; set; }
    [Export] public Node2D entityRoot { get; set; }
    [Export] public Node2D effectRoot { get; set; }

    // The ui will have their classes changed once they are made so you can easily call their functions
    [Export] public Control hudRoot { get; set; }
    [Export] public Control pauseRoot { get; set; }
    [Export] public Control transitionRoot { get; set; }
    [Export] public PackedScene playerScene;

    

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        GameManager.Instance.MainGameReady();
    }

    public async Task LoadLevel(PackedScene levelScene, Player player)
    {
        // await transition, fade to black or something
        ClearLevel();
        Node2D level = levelScene.Instantiate<Node2D>();
        levelRoot.AddChild(level);
        if (player == null)
        {
            player = playerScene.Instantiate<Player>();
            player.availableColors = Mathf.Min(GameManager.Instance.zoneIndex, 2);
            entityRoot.AddChild(player);
            player.TeleportToSpawnPoint(level.GetNode<Marker2D>("PlayerSpawn"));
        }

        // await transition, fade in or something else
    }


    public void ClearLevel()
    {
        foreach(Node2D entity in entityRoot.GetChildren())
        {
            entityRoot.RemoveChild(entity);
            entity.QueueFree();
        }

        foreach(Node2D level in levelRoot.GetChildren())
        {
            levelRoot.RemoveChild(level);
            level.QueueFree();
        }

        foreach(Node2D groundVFX in groundVFXRoot.GetChildren())
        {
            groundVFXRoot.RemoveChild(groundVFX);
            groundVFX.QueueFree();
        }

        foreach(Node2D effect in effectRoot.GetChildren())
        {
            effectRoot.RemoveChild(effect);
            effect.QueueFree();
        }

    }
}
