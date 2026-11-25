using Godot;
using System;

public partial class Enemy : Character
{
	//[Export] public Texture2D spriteSheet;
	[Export] public int frameSize = 5;
	[Export] public float speed = 80f;
	[Export] public float attackRange = 200f;
	[Export] public float attackCooldown = 5f;
	[Export] public int attackDamage = 1;
	[Export] public int attackHitFrame = 2; 
	[Export] public int deathEffectFrame = 3; 
	[Export] public bool canBeKnockedBack = true;
	[Export] public float knockbackForce = 5f;

	protected EnemyState state;
	protected AnimatedSprite2D sprite;
	protected NavigationAgent2D navAgent;
	protected Player player => MainGame.Instance?.player;

	private float attackTimer;
	private Vector2 knockVelocity;

	public override void _Ready()
	{
		base._Ready();
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		//GameManager.Instance.enemiesAlive++;
		state = EnemyState.Chase;
		sprite.FrameChanged += OnFrameChanged;
		sprite.AnimationFinished += OnAnimationFinished;
		PlayAnim("WALK");
	}

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

	public override void TakeDamage(int amount, Vector2 hitFrom = default)
	{
		if(state == EnemyState.Dead)
		{
			return;
		}
		base.TakeDamage(amount, hitFrom);
		if(state == EnemyState.Chase)
		{
			PlayAnim("HURT");
		}
		if(state == EnemyState.Dead)
		{
			return;
		}
	}

	protected override void Die()
	{
		state = EnemyState.Dead;
		Velocity = Vector2.Zero;
		PlayAnim("DEATH");
		//GameManager.kills++;
		//GameManager.enemiesAlive--;
	} 

	protected virtual void Tick(double delta)
	{
		if (attackTimer > 0)
		{
			attackTimer -= (float)delta;
		}
	}

	protected virtual void Think()
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
		if(state == EnemyState.Attack && anim == "ATTACK" && sprite.Frame == attackHitFrame)
		{
			OnAttackStart();
		}
	}
	
	private void OnAnimationFinished()
	{
		switch (sprite.Animation.ToString())
		{
			case "ATTACK":
				if(state == EnemyState.Attack)
				{
					state = EnemyState.Chase;
				}
				break;


			case "HURT":
				if(state == EnemyState.Chase)
				{
					PlayAnim("WALK");
				}
				break;

			case "DEATH":
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
