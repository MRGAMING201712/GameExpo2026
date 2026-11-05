using Godot;
using System;
using System.Collections.Generic;


public enum PaintColor
{
    None,
    Red,
    Blue,
    Yellow
}
public partial class PaintManager : Node
{
    public static PaintManager Instance { get; private set; }
    [Export] public int cellSize = 4;
    [Export] public float coverageEmitStep = 0.005f; // how much coverage, as a percentage, must change before emitting the CoverageChanged signal
    [Export] public Texture2D [] brushTextures = { };

    // Paint Colors
    public Color [] palette =
    {
        new Color(0, 0, 0, 0), // Transparent
        Colors.Red,
        Colors.Blue,
        Colors.Yellow
    };

    public ImageTexture maskTexture { get; private set; }
    public float coverage { get; private set; }

    // Reading the gameplay copy vs the visual copy
    private Image image;
    private PaintColor [] cells; // gameplay array of the image, holds the enum values of palette for each cell
    private bool [] blocked; // makes it so if there are buildings or walls, it doesn't count to the overall coverage, just the floor counts.
    private int width, height;
    private Rect2 zoneRect;
    private int paintable, paintedCount;
    private bool imageChanged; 
    private float lastCoverageEmitted = -1f;
    
    // Brushes
    private Image [] brushSources = { };
    private Dictionary<(int brush, PaintColor color, int size, int variation), Image> brushCache = new();
    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
        brushSources = new Image[brushTextures.Length];
        for(int i = 0; i < brushTextures.Length; i++)
        {
            Image brushImage = brushTextures[i].GetImage();
            brushImage.Convert(Image.Format.Rgba8);
            brushSources[i] = brushImage;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if(!imageChanged)
        {
            return;
        }
        imageChanged = false;
        maskTexture.Update(image);
        coverage = paintable > 0 ? (float)paintedCount / paintable : 0f;
        EmitCoverage();
    }
    
    public void Paint(Vector2 worldPos, float radius, PaintColor color)
    {
        if (color == PaintColor.None) 
        {
             Erase(worldPos, radius); 
             return; 
        }
        if (brushSources.Length == 0) 
        {
             StampCircle(worldPos, radius, color); 
             return; 
        }

        int size = Mathf.Max(2, Mathf.RoundToInt(radius * 2f / cellSize));
        int brush = GD.RandRange(0, brushSources.Length - 1);
        int variant = GD.RandRange(0, 7);
        Image stamp = GetBrushImage(brush, color, size, variant);

        Vector2I center = WorldToCell(worldPos);
        Vector2I topLeft = center - new Vector2I(size / 2, size / 2);

        image.BlendRect(stamp, new Rect2I(Vector2I.Zero, stamp.GetSize()), topLeft);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            if (stamp.GetPixel(x, y).A < 0.5f)
            {
              continue;  
            } 
            SetCell(topLeft.X + x, topLeft.Y + y, color);
        }
        imageChanged = true;
    }

    // makes a blank canvas when scene loads
    public void SetupZone(Rect2 inZoneRect)
    {
        zoneRect = inZoneRect;
        width = Mathf.CeilToInt(zoneRect.Size.X / cellSize);
        height = Mathf.CeilToInt(zoneRect.Size.Y / cellSize);

        cells = new PaintColor[width * height];
        blocked = new bool[width * height];
        paintable = width * height; // how many cells count to coverage
        paintedCount = 0;
        coverage = 0f;
        lastCoverageEmitted = -1f;

        image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(palette[0]);
        maskTexture = ImageTexture.CreateFromImage(image);
        RenderingServer.GlobalShaderParameterSet("paint_mask", maskTexture);
        RenderingServer.GlobalShaderParameterSet("paint_zone",new Vector4(zoneRect.Position.X, zoneRect.Position.Y, zoneRect.Size.X, zoneRect.Size.Y)); 
        EmitCoverage(true);
    }

    // Grey to color = ++, color to grey = --, color to color = nothing
    private void SetCell(int cellX, int cellY, PaintColor newValue)
    {
        if(cellX < 0 || cellX >= width || cellY < 0 || cellY >= height)
        {
            return;
        }
        
        int index = cellY * width + cellX;
        PaintColor oldValue = cells[index];
        if(oldValue == newValue)
        {
            return;
        }

        cells[index] = newValue;
        if(blocked[index])
        {
            return;
        }

        if(oldValue == 0)
        {
            paintedCount++;
        }
        else if(newValue == 0)
        {
            paintedCount--;
        }
    }

    void StampCircle(Vector2 worldPos, float radius, PaintColor color)
    {
        Vector2I centerCell = WorldToCell(worldPos);
        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        int cellRadiusSq = cellRadius * cellRadius;

        for(int y = -cellRadius; y <= cellRadius; y++)
        {
            for(int x = -cellRadius; x <= cellRadius; x++)
            {
                if(x * x + y * y > cellRadiusSq)
                {
                    continue;
                }

                int cellX = centerCell.X + x;
                int cellY = centerCell.Y + y;
                if(cellX < 0 || cellX >= width || cellY < 0 || cellY >= height)
                {
                    continue;
                }

                image.SetPixel(cellX, cellY, palette[(int)color]);
                SetCell(cellX, cellY, color);
            }
        }
        imageChanged = true;
    }

    public void Erase(Vector2 worldPos, float radius)
    {
       StampCircle(worldPos, radius, PaintColor.None);  
    }

    public void EmitCoverage(bool bForceChange = false)
    {
        if (!bForceChange && Mathf.Abs(coverage - lastCoverageEmitted) < coverageEmitStep)
        {
            return;
        }

        lastCoverageEmitted = coverage;
        EventManager.Instance.EmitSignal(EventManager.SignalName.CoverageChanged, coverage);
    }

    // might not need this as we might not need to block areas, if we need it this is how we would do it
    public void MarkBlockedArea(Rect2 worldRect)
    {
        Vector2I minCell = WorldToCell(worldRect.Position);
        Vector2I maxCell = WorldToCell(worldRect.End);

        for(int y = Mathf.Max(minCell.Y, 0); y <= Mathf.Min(maxCell.Y, height); y++)
        {
            for(int x = Mathf.Max(minCell.X, 0); x <= Mathf.Min(maxCell.X, width); x++)
            {
                int index = y * width + x;
                if(blocked[index])
                {
                    continue;
                }

                blocked[index] = true;
                paintable--;
                if(cells[index] != 0)
                {
                    paintedCount--;
                }
            }
        }
    }

    private Image GetBrushImage(int brushIndex, PaintColor color, int size, int variation)
    {
        var key = (brushIndex, color, size, variation);
        if(brushCache.TryGetValue(key, out Image cachedImage))
        {
            return cachedImage;
        }

        Image brushImage = (Image)brushSources[brushIndex].Duplicate();
        brushImage.Resize(size, size, Image.Interpolation.Bilinear); // switch interpolation for testing
        if (GD.Randi() % 2 == 0) 
        {
            brushImage.FlipX();
        }
        if (GD.Randi() % 2 == 0)
        {
            brushImage.FlipY();            
        }
        if (GD.Randi() % 2 == 0)
        {
            brushImage.Rotate90(ClockDirection.Clockwise);  
        } 

        Color tint = palette[(int)color];
        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                float alpha = brushImage.GetPixel(x, y).A;
                brushImage.SetPixel(x, y, new Color(tint.R, tint.G, tint.B, alpha));
            }
        }

        brushCache[key] = brushImage;
        return brushImage;
    }

    // Gameplay Helpers

    public PaintColor GetCellColor(Vector2 worldPos)
    {
        Vector2I cell = WorldToCell(worldPos);
        if(!bInBounds(cell))
        {
            return PaintColor.None;
        }
        return cells[cell.Y * width + cell.X];
    }

    public bool IsCellPainted(Vector2 worldPos)
    {
        Vector2I cell = WorldToCell(worldPos);
        if(!bInBounds(cell))
        {
            return false;
        }
        return cells[cell.Y * width + cell.X] != PaintColor.None;
    }

    public bool FindUnpaintedCell(Rect2 area, out Vector2 worldPos, int tries = 20)
    {
        for(int i = 0; i < tries; i++)
        {
            Vector2 randomPos = new Vector2((float)GD.RandRange(area.Position.X, area.End.X), (float)GD.RandRange(area.Position.Y, area.End.Y));
            Vector2I cell = WorldToCell(randomPos);
            if(!bInBounds(cell))
            {
                continue;
            }
            int index = cell.Y * width + cell.X;
            if(!blocked[index] && cells[index] == PaintColor.None)
            {
                worldPos = randomPos;
                return true;
            }
        }
        worldPos = Vector2.Zero;
        return false;
    }

    private Vector2I WorldToCell(Vector2 worldPos)
    {
        Vector2 localPos = worldPos - zoneRect.Position;
        return new Vector2I(Mathf.FloorToInt(localPos.X / cellSize), Mathf.FloorToInt(localPos.Y / cellSize));
    }

    private bool bInBounds(Vector2I cell)
    {
        return cell.X >= 0 && cell.X < width && cell.Y >= 0 && cell.Y < height;
    }
}
