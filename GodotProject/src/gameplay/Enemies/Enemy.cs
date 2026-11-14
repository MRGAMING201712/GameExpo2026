using Godot;
using System;

public partial class Enemy : Character
{
	[Export] public Texture2D spriteSheet;
	[Export] public int frameSize;
	[Export] public float speed;
	[Export] public float attackRange;
	[Export] public float attackCooldown;
	[Export] public int attackDamage;
	[Export] public int attackHitFrame; 
	[Export] public int deathEffectFrame; 
	[Export] public bool canBeKnockedBack;
	[Export] public float knockbackForce;

	protected EnemyState state;
	protected AnimatedSprite2D sprite;
	protected NavigationAgent2D navAgent;
	protected Player player => MainGame.Instance.player;

	private float attackTimer;
	private Vector2 knockVelocity;

	public override void _Ready()
	{
		base._Ready();
		//GameManager.Instance.enemiesAlive++;
		state = EnemyState.Chase;
		sprite.FrameChanged += OnFrameChanged;
		sprite.AnimationFinished += OnAnimationFinished;
		PlayAnim("WALK");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(state != EnemyState.Dead)
		{
			Tick(delta);
		}

		switch(state)
		{
			case EnemyState.Chase:
				Think();
				TryAttack();
				break;

			case EnemyState.Attack:
				Velocity = AttackMovement();
				break;

			case EnemyState.Dead:
				Velocity = Vector2.Zero;
				break;
		}
		MoveAndSlide();
	}

	public void TakeDamage(int amount, Vector2 hitFrom = default)
	{
		if(state == EnemyState.Dead)
		{
			return;
		}

		base.TakeDamage(amount, hitFrom);
	}

	protected void Die()
	{
		state = EnemyState.Dead;
		Velocity = Vector2.Zero;

		//GameManager.kills++;
		//GameManager.enemiesAlive--;
	}

	protected void Tick(double delta)
	{
		if (attackTimer > 0)
		{
			attackTimer -= (float)delta;
		}
	}

	protected void Think()
	{
		ChasePlayer(speed);
	}

	protected virtual Vector2 AttackMovement()
	{
		return Vector2.Zero;
	}

	protected virtual void OnAttackStart()
	{
	}

	protected virtual void OnDeathEffect()
	{
	}

	protected void MoveToward(Vector2 target, float spd)
	{
		if(navAgent.TargetPosition.DistanceTo(target) > 48)
		{
			navAgent.TargetPosition = target;
		}

		Velocity = GlobalPosition.DirectionTo(target) * spd;		
	}

	protected void ChasePlayer(float spd) 
	{
		MoveToward(player.GlobalPosition, spd);
		FaceToward(player.GlobalPosition);
	}

	protected void FaceToward(Vector2 point)
	{
		facingLeft = point.X < GlobalPosition.X;
		sprite.FlipH = facingLeft;
	}

	protected void PlayAnim(string anim)
	{
		if(sprite.Animation.ToString() == anim && sprite.IsPlaying())
		{
			return;
		}
		sprite.Play(anim);
	}

	protected bool TryAttack()
	{
		if(attackTimer > 0f)
		{
			return false;
		}
		if(GlobalPosition.DistanceTo(player.GlobalPosition) > attackRange)
		{
			return false;
		}
		StartAttack();
		return true;
	}

	protected void StartAttack()
	{
		state = EnemyState.Attack;
		attackTimer = attackCooldown;
		Velocity = Vector2.Zero;
		FaceToward(player.GlobalPosition);
		OnAttackStart();
		PlayAnim("ATTACK");
	}

	protected void HitFlash()
	{
		PlayAnim("HURT");
	}

	private void UpdateLocomotionAnim()
	{
		if(state == EnemyState.Chase)
		{
			return;
		}
		if(Velocity.LengthSquared() > 1f)
		{
			PlayAnim("WALK");
		} else {
			PlayAnim("IDLE");
		}
	}

	private void OnFrameChanged()
	{
		string anim = sprite.Animation.ToString();
		if(state == EnemyState.Attack && anim == "attack" && sprite.Frame == attackHitFrame)
		{
			OnAttackStart();
		}
	}

	private void OnAnimationFinished()
	{
		switch (sprite.Animation.ToString())
		{
			case "attack":
				if(state == EnemyState.Attack)
				{
					state = EnemyState.Chase;
				}
				break;

				case "death":
					QueueFree();
					break;
		}
	}
}

public enum EnemyState
{
	Chase,
	Attack,
	Knockback,
	Dead
}
