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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
	}

	public void TakeDamage(int amount, Vector2 hitFrom = default)
	{
	}

	protected void Die()
	{
	}

	protected void Tick()
	{
	}

	protected void Think()
	{
	}

	protected Vector2 AttackMovement()
	{
	}

	protected void OnAttackStart()
	{
	}

	protected void OnDeathEffect()
	{
	}

	protected void MoveToward(Vector2 target, float spd)
	{
	}

	protected void ChasePlayer(float spd) 
	{
	}

	protected void FaceToward(Vector2 point)
	{
	}

	protected void PlayAnim(string anim)
	{
	}

	protected bool TryAttack()
	{
	}

	protected void StartAttack()
	{
	}

	protected void HitFlash()
	{
	}

	private void UpdateLocomotionAnim()
	{
	}

	private void OnFrameChanged()
	{
	}

	private void OnAnimationFinished()
	{
	}
}
