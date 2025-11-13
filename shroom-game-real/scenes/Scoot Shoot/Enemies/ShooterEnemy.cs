using Godot;
using ShroomGameReal.Tv.GameStates;

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
    
    [Export]
    public float attackDelay = 1f;
    
    [Export]
    public Node3D attackSpot;
    
    public HealthComponent HealthComponent { get; private set; }

    public ScootShootOnRailsGame game;
    public ScootShootStage stage;
    
    private TvGameTimer _damagePlayerTimer;
    private RandomNumberGenerator _rng = new();
    
    private Vector3 _restPosition;
    private Tween _tween;

    public override void _Ready()
    {
        HealthComponent = GetNode<HealthComponent>("HealthComponent");
        HealthComponent.OnDeath += OnDeath;
        
        _damagePlayerTimer = GetNode<TvGameTimer>("Damage Player Timer");
        _damagePlayerTimer.Timeout += AttackPlayer;
        _damagePlayerTimer.OneShot = true;
        _damagePlayerTimer.WaitTime = GetNextShotTime();

        _restPosition = GlobalPosition;
    }

    public void StartCombat()
    {
        _damagePlayerTimer.Start();
    }

    private void OnDeath()
    {
        _tween?.Kill();
        _damagePlayerTimer.Stop();
        QueueFree();
    }

    private float GetNextShotTime() => _rng.RandfRange(minTimeBetweenShots, maxTimeBetweenShots);

    private void AttackPlayer()
    {
        if (HealthComponent.IsDead)
            return;
        
        _tween?.Kill();

        _tween = CreateTween();
        _tween.TweenProperty(this, "global_position", attackSpot.GlobalPosition, 0.5f);
        _tween.TweenInterval(attackDelay);
        _tween.TweenCallback(Callable.From(() =>
        {
            game.DamagePlayer(_rng.RandfRange(minDamage, maxDamage));
            
            _damagePlayerTimer.WaitTime = GetNextShotTime();
            _damagePlayerTimer.Start();
        }));
        _tween.TweenProperty(this, "global_position", _restPosition, 0.5f);
    }
}