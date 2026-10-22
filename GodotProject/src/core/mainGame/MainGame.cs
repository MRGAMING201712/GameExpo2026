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
    [Export] public LevelTransition transitionRoot { get; set; }
    [Export] public PackedScene playerScene;

    public Player player { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        GameManager.Instance.MainGameReady();
    }

    public void LoadLevel(PackedScene levelScene)
    {
        ClearLevel(keep: player);

        Node2D level = levelScene.Instantiate<Node2D>();
        levelRoot.AddChild(level);

        if (player == null)
        {
            player = playerScene.Instantiate<Player>();
            entityRoot.AddChild(player);
        }

        player.availableColors = Mathf.Min(GameManager.Instance.zoneIndex, 2);
        player.TeleportToSpawnPoint(level.GetNode<Marker2D>("PlayerSpawn"));
    }

    public void ClearLevel(Node keep = null)
    {
        foreach (Node root in new Node[] { entityRoot, levelRoot, groundVFXRoot, effectRoot })
        {
            foreach (Node child in root.GetChildren())
            {
                if (child == keep)
                {
                  continue; // don't delete the player  
                } 
                root.RemoveChild(child);
                child.QueueFree();
            }
        }
    }
}