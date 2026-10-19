using Godot;
using System;

public partial class ButtonTest : Button
{
    [Export] PackedScene mainGame;
    public override void _Ready()
    {
        this.Pressed += OnPressed; 
    }

    public void OnPressed()
    {
        GetTree().ChangeSceneToPacked(mainGame);
    }
}
