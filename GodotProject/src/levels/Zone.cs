using Godot;

public partial class Zone : Node2D
{
    public override void _Ready()
    {
        Setup();
    }

    public void Setup()
    {
        TileMapLayer floor = GetNode<TileMapLayer>("%Floor");
        TileMapLayer buildings = GetNode<TileMapLayer>("%Buildings");

        PaintManager.Instance.SetupZone(GetLayerRect(floor));

        foreach (Vector2I cell in buildings.GetUsedCells())
        {
            PaintManager.Instance.MarkBlockedArea(GetCellRect(buildings, cell));
        }
    }

    private static Rect2 GetCellRect(TileMapLayer layer, Vector2I cell)
    {
        Vector2 tileSize = (Vector2)layer.TileSet.TileSize * layer.GlobalScale;
        Vector2 center = layer.ToGlobal(layer.MapToLocal(cell));
        return new Rect2(center - tileSize / 2f, tileSize);
    }

    private static Rect2 GetLayerRect(TileMapLayer layer)
    {
        Rect2I used = layer.GetUsedRect();
        Vector2 tileSize = (Vector2)layer.TileSet.TileSize * layer.GlobalScale;
        Vector2 topLeft = layer.ToGlobal(layer.MapToLocal(used.Position)) - tileSize / 2f;
        Vector2 size = new Vector2(used.Size.X, used.Size.Y) * tileSize;
        return new Rect2(topLeft, size);
    }
}