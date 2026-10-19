using Godot;
using System;

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


    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        // gameManager will have a on main game ready function
    }
}
