using Godot;
using System;
using System.Runtime.InteropServices;

public partial class Player : CharacterBody2D
{
    [Export] public HealthComponent health { get; private set; }

    // Movement
    [Export] public float speed { get; set; } = 300.0f;
    [Export] public float acceleration { get; set; } = 1800.0f; // how fast to reach top speed
    [Export] public float friction { get; set; } = 2200.0f; // how fast to stop

    // painting(attacking)
    [Export] public float arcLength = 1.2f;
    [Export] public float arcDistanceFromPlayer = 56f;
    [Export] public float timeForSwing = 1f;
    [Export] public float paintSize = 18f;
    private bool bIsSwinging = false;    
    private float aim;
    private Vector2 lastTipLocation;
    [Export] public Node2D brushRoot;
    [Export] public Sprite2D brushTip;
    public PaintColor [] allColors = {PaintColor.Red, PaintColor.Blue, PaintColor.Yellow};
    public int availableColors = 0;
    public int currentColor = 0;



    public override void _Ready()
    {
        health.HealthChanged += OnHealthChanged;
        health.Died += OnDied;
        brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
    }

    public override void _ExitTree()
    {
        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnDied;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("attack") && !bIsSwinging)
        {
            bIsSwinging = true;
            brushRoot.Visible = true;
            aim = (GetGlobalMousePosition() - GlobalPosition).Angle();
            lastTipLocation = TipAt(-arcLength);

            Tween tween = CreateTween();
            tween.TweenMethod(Callable.From<float>(SwingStep), -arcLength, arcLength, timeForSwing).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenCallback(Callable.From(EndSwing));
        }

        if(@event.IsActionPressed("switchPaintUp"))
        {
            if(currentColor + 1 >= allColors.Length || currentColor + 1 > availableColors)
            {
                currentColor = 0;
                brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
                return;
            }
            currentColor++;
            brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
        }

        if(@event.IsActionPressed("switchPaintDown"))
        {            
            if(currentColor - 1 < 0)
            {
                currentColor = Mathf.Min(availableColors, allColors.Length - 1);
                brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
                return;
            }
            currentColor--;
            brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
        }
    
    
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        
        if(inputDirection != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(inputDirection * speed, acceleration * (float)delta);
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, friction * (float)delta);
        }
        MoveAndSlide();
    }

    public void TeleportToSpawnPoint(Marker2D playerSpawn)
    {
        this.GlobalPosition = playerSpawn.GlobalPosition;
    }
    private void SwingStep(float offset)
    {            
        aim = (GetGlobalMousePosition() - GlobalPosition).Angle();

        brushRoot.Rotation = aim + offset;
        Vector2 tip = TipAt(offset);

        int steps = Mathf.Max(1, Mathf.CeilToInt(lastTipLocation.DistanceTo(tip) / (paintSize * 0.5f)));
        for (int i = 1; i <= steps; i++)
        {
            PaintManager.Instance.Paint(lastTipLocation.Lerp(tip, (float)i / steps), paintSize, allColors[currentColor]);
        }

        lastTipLocation = tip;
    }

    private Vector2 TipAt(float offset)
    {
        return GlobalPosition + Vector2.FromAngle(aim + offset) * arcDistanceFromPlayer;
    }

    void EndSwing()
    {
        brushRoot.Visible = false;
        bIsSwinging = false;
    }

    private void OnHealthChanged(int current, int max)
    {
        EventManager.Instance.EmitSignal("PlayerHealthChanged", current, max);
    }

    private void OnDied()
    {
        EventManager.Instance.EmitSignal("PlayerDied");
    }
}
