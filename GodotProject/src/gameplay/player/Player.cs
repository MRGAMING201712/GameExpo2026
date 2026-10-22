using Godot;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class Player : CharacterBody2D
{
    [Export] public HealthComponent health { get; private set; }

    // Movement
    [Export] public float speed { get; set; } = 300.0f;
    [Export] public float acceleration { get; set; } = 1800.0f;
    [Export] public float friction { get; set; } = 2200.0f;

    // painting(attacking)
    [Export] public float arcLength = 1.2f;
    [Export] public float arcDistanceFromPlayer = 56f;
    [Export] public float timeForSwing = 1f;
    [Export] public float paintSize = 18f;
    private bool bIsSwinging = false;
    private Tween swingTween;
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
        // No swinging or color switching during transitions, menus, death, etc.
        if (!GameManager.Instance.CanControlPlayer)
        {
            return;  
        } 

        if (@event.IsActionPressed("attack") && !bIsSwinging)
        {
            bIsSwinging = true;
            brushRoot.Visible = true;
            aim = (GetGlobalMousePosition() - GlobalPosition).Angle();
            lastTipLocation = TipAt(-arcLength);

            swingTween = CreateTween();
            swingTween.TweenMethod(Callable.From<float>(SwingStep), -arcLength, arcLength, timeForSwing).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            swingTween.TweenCallback(Callable.From(EndSwing));
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
        if (!GameManager.Instance.CanControlPlayer)
        {
            Velocity = Vector2.Zero; // stop movement during transitions
            return;
        }
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
        // stop swinging during level transitions
        swingTween?.Kill();
        EndSwing();

        Velocity = Vector2.Zero;
        GlobalPosition = playerSpawn.GlobalPosition;
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