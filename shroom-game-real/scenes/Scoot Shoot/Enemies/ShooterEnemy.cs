using Godot;

namespace ShroomGameReal.scenes.Scoot_Shoot.Enemies;

[GlobalClass]
public partial class ShooterEnemy : RigidBody3D, IHealthProvider
{
    [Export]
    public float minDamage = 3f;

    [Export]
    public float maxDamage = 10f;

    [Export]
    public float minTimeBetweenShots = 3f;

    [Export]
    public float maxTimeBetweenShots = 7f;
    
    public HealthComponent HealthComponent { get; private set; }

    public ScootShootOnRailsGame game;
    public ScootShootStage stage;
    
    private Timer _damagePlayerTimer;
    private RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        HealthComponent = GetNode<HealthComponent>("HealthComponent");
        HealthComponent.OnDeath += OnDeath;
        
        _damagePlayerTimer = GetNode<Timer>("Damage Player Timer");
        _damagePlayerTimer.Timeout += DealDamageToPlayer;
        _damagePlayerTimer.OneShot = true;
        _damagePlayerTimer.WaitTime = GetNextShotTime();
    }

    public void StartCombat()
    {
        _damagePlayerTimer.Start();
    }

    private void OnDeath()
    {
        _damagePlayerTimer.Stop();
        QueueFree();
    }

    private float GetNextShotTime() => _rng.RandfRange(minTimeBetweenShots, maxTimeBetweenShots);

    private void DealDamageToPlayer()
    {
        if (HealthComponent.IsDead)
            return;
        
        game.DamagePlayer(_rng.RandfRange(minDamage, maxDamage));
        
        _damagePlayerTimer.WaitTime = GetNextShotTime();
        _damagePlayerTimer.Start();
    }
}