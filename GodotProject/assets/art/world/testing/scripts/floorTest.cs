using Godot;
using System;

public partial class floorTest : Sprite2D
{
    public override void _Ready()
    {
        Rect2 rect = GetRect(); 
        PaintManager.Instance.SetupZone(new Rect2(GlobalPosition + rect.Position, rect.Size));
        ((ShaderMaterial)Material).SetShaderParameter("paint_mask", PaintManager.Instance.maskTexture);
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        if (Input.IsMouseButtonPressed(MouseButton.Left))  PaintManager.Instance.Paint(mousePos, 40f, PaintColor.Red);
        if (Input.IsMouseButtonPressed(MouseButton.Right)) PaintManager.Instance.Erase(mousePos, 40f);
        if (Input.IsKeyPressed(Key.Space)) 
        {
            GD.Print($"Coverage: {PaintManager.Instance.coverage:P1}");
        }
    }
}
