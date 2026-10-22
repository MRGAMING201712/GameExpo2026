using Godot;
using System;

public partial class Zone : Node2D
{
    public override void _Ready()
    {
        Sprite2D floor = GetNode<Sprite2D>("%Floor");
        Rect2 rect = floor.GetRect();
        Rect2 zoneRect = new Rect2(floor.GlobalPosition + rect.Position, rect.Size);
        PaintManager.Instance.SetupZone(zoneRect);

        foreach(Node child in GetNode("%Buildings").GetChildren())
        {
            CollisionShape2D collisionShape = child.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if(collisionShape != null && collisionShape.Shape is RectangleShape2D rectangleShape)
            {
                Vector2 size = rectangleShape.Size;
                PaintManager.Instance.MarkBlockedArea(new Rect2(collisionShape.GlobalPosition - size / 2, size));
            }
        }
    }

    
}
