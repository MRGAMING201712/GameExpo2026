using Godot;
using System;
using System.Collections.Generic;
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
	[Export] private Area2D brushHitbox;
	[Export] public int brushDamage = 1;
	private readonly List<Enemy> hitThisSwing = new();


	// Animations
	[Export] public AnimatedSprite2D sprite;
	[Export] public bool matchSwingAnimToSwingTime = true; // stretches the swing anim
	[Export] public float walkAnimThreshold = 10f;  // idle threshold, might remove and make it based on input but depends how acceleration and friction feel

	private enum FacingDir 
	{ 
		Down, 
		Side, 
		Up 
	}
	private FacingDir facing = FacingDir.Down;
	private bool facingLeft = false;

	private bool bIsHurt = false;
	private bool bIsDead = false;
	private bool bIsCelebrating = false;
	private int lastHealth = int.MaxValue;

	public override void _Ready()
	{
		health.HealthChanged += OnHealthChanged;
		health.Died += OnDied;
		sprite.AnimationFinished += OnAnimationFinished;
		brushTip.Modulate = PaintManager.Instance.palette[currentColor + 1]; // cuz palette has transluscent as 0
		brushHitbox.Monitoring = false;
    	brushHitbox.AreaEntered += OnBrushHit;
		PlayAnim(DirAnim("idle"));
	}

	public override void _ExitTree()
	{
		health.HealthChanged -= OnHealthChanged;
		health.Died -= OnDied;
		sprite.AnimationFinished -= OnAnimationFinished;
		brushHitbox.AreaEntered -= OnBrushHit;
	}
	public override void _Input(InputEvent @event)
	{
		if (!GameManager.Instance.bCanControlPlayer || bIsDead || bIsCelebrating)
		{
			return;
		} 

		if (@event.IsActionPressed("attack") && !bIsSwinging)
		{
			bIsSwinging = true;
			bIsHurt = false;
			brushRoot.Visible = true;
			hitThisSwing.Clear();
			brushHitbox.SetDeferred(Area2D.PropertyName.Monitoring, true);
			aim = (GetGlobalMousePosition() - GlobalPosition).Angle();
			lastTipLocation = TipAt(-arcLength);
			FaceToward(GetGlobalMousePosition());
			StartSwingAnim();

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
		if (bIsDead || bIsCelebrating || !GameManager.Instance.bCanControlPlayer)
		{
			Velocity = Vector2.Zero;
			UpdateAnimation(Vector2.Zero);
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
		UpdateAnimation(inputDirection);
	}

	public void TeleportToSpawnPoint(Marker2D playerSpawn)
	{
		// stop swinging during level transitions
		swingTween?.Kill();
		EndSwing();
		bIsHurt = false;
		bIsCelebrating = false;

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
		sprite.SpeedScale = 1f;
		brushHitbox.SetDeferred(Area2D.PropertyName.Monitoring, false);
		hitThisSwing.Clear();
	}

	private void OnHealthChanged(int current, int max)
	{
		if (!bIsDead && !bIsSwinging && current < lastHealth && current < max && current > 0)
		{
			bIsHurt = true;
			sprite.FlipH = false;
			sprite.Play("hurt"); // restart if already playing
		}
		lastHealth = current;
		EventManager.Instance.EmitSignal("PlayerHealthChanged", current, max);
	}

	private void OnDied()
	{
		bIsDead = true;
		bIsHurt = false;
		swingTween?.Kill();
		EndSwing();
		Velocity = Vector2.Zero;
		sprite.FlipH = false;
		PlayAnim("death");
		EventManager.Instance.EmitSignal("PlayerDied");
	}

	public void PlayVictory()
	{
		if (bIsDead) return;
		swingTween?.Kill();
		EndSwing();
		bIsHurt = false;
		bIsCelebrating = true;
		PlayAnim("victory");
	}

	// Attacking Logic
	private void OnBrushHit(Area2D area)
	{
		if (!bIsSwinging) return;
		if (area.Owner is not Enemy enemy) return;
		if (hitThisSwing.Contains(enemy)) return;
		hitThisSwing.Add(enemy);
		enemy.TakeDamage(brushDamage, GlobalPosition);
	}
	// Animation logic
	// Priority: dead > victory > swing > hurt > walk/idle
	private void UpdateAnimation(Vector2 inputDirection)
	{
		if (bIsDead || bIsCelebrating)
		{
			return;
		} 
		if (bIsSwinging) 
		{
			return;
		}

		if (bIsHurt)
		{
			PlayAnim("hurt");
			return;
		}

		if (inputDirection != Vector2.Zero)
		{
			SetFacing(inputDirection);
		}

		if (Velocity.LengthSquared() > walkAnimThreshold * walkAnimThreshold)
			{
				PlayAnim(DirAnim("walk"));
			}
		else
			{
				PlayAnim(DirAnim("idle"));
			}
	}

	private void StartSwingAnim()
	{
		string anim = DirAnim("swing");
		sprite.SpeedScale = 1f;

		if (matchSwingAnimToSwingTime && timeForSwing > 0f)
		{
			float animLength = GetAnimLength(anim);
			if (animLength > 0f)
			{
				sprite.SpeedScale = animLength / timeForSwing;
			}
		}
		sprite.Play(anim);
	}

	private float GetAnimLength(string anim)
	{
		SpriteFrames frames = sprite.SpriteFrames;
		if (frames == null || !frames.HasAnimation(anim))
		{
			return 0f;
		} 

		double fps = frames.GetAnimationSpeed(anim);
		if (fps <= 0) 
		{
			return 0f;
		}

		double total = 0;
		int count = frames.GetFrameCount(anim);
		for (int i = 0; i < count; i++)
		{
			total += frames.GetFrameDuration(anim, i);
		}
		return (float)(total / fps);
	}

	private void FaceToward(Vector2 point)
	{
		SetFacing(point - GlobalPosition);
	}

	private void SetFacing(Vector2 dir)
	{
		if (dir == Vector2.Zero)
		{
			return;
		} 

		if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
		{
			facing = FacingDir.Side;
			facingLeft = dir.X < 0f;
		}
		else
		{
			facing = dir.Y < 0f ? FacingDir.Up : FacingDir.Down;
		}
		sprite.FlipH = facing == FacingDir.Side && facingLeft;
	}

	private string DirAnim(string baseName)
	{
		return facing switch
		{
			FacingDir.Up => baseName + "_up",
			FacingDir.Side => baseName + "_side",
			_ => baseName + "_down",
		};
	}

	private void PlayAnim(string anim)
	{
		if (sprite.Animation.ToString() == anim && sprite.IsPlaying())
		{
			return;
		}
		sprite.Play(anim);
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation.ToString() == "hurt")
			{
				bIsHurt = false;
			} 
	}
}
