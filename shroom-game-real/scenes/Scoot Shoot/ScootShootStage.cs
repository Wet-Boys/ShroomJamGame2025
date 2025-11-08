using Godot;
using ShroomGameReal.scenes.Scoot_Shoot.Enemies;

namespace ShroomGameReal.scenes.Scoot_Shoot;

[GlobalClass]
public partial class ScootShootStage : Node3D
{
    [Export]
    public ShooterEnemy[] enemies = [];
    
    [Export]
    public double introDuration = 2.0;

    public PathFollow3D pathFollower;
    public RemoteTransform3D transformProxy;

    [Signal]
    public delegate void OnStageStartEventHandler();

    [Signal]
    public delegate void OnStageFinishedEventHandler();
    
    public bool Started { get; private set; }

    private ScootShootOnRailsGame _game;

    private uint _deadEnemies;

    public override void _Ready()
    {
        _game = GetNode<ScootShootOnRailsGame>("../../");
        pathFollower = GetNode<PathFollow3D>("Intro Path/Path Follower");
        transformProxy = pathFollower.GetNode<RemoteTransform3D>("RemoteTransform3D");

        foreach (var enemy in enemies)
        {
            enemy.HealthComponent.OnDeath += EnemyDied;
            enemy.game = _game;
            enemy.stage = this;
        }

        OnStageStart += () =>
        {
            foreach (var enemy in enemies)
                enemy.StartCombat();
            
            Started = true;
        };
    }

    public void EnterStage()
    {
        transformProxy.UpdatePosition = true;
        transformProxy.UpdateRotation = true;
        transformProxy.UpdateScale = true;

        var tween = CreateTween();
        tween.TweenProperty(pathFollower, "progress_ratio", 1.0, introDuration).SetEase(Tween.EaseType.InOut);
        tween.TweenCallback(Callable.From(EmitSignalOnStageStart));
    }

    private void EnemyDied()
    {
        _deadEnemies++;

        if (_deadEnemies >= enemies.Length)
            FinishStage();
    }

    private void FinishStage()
    {
        GD.Print("Stage finished");
        transformProxy.UpdatePosition = false;
        transformProxy.UpdateRotation = false;
        transformProxy.UpdateScale = false;
        
        EmitSignalOnStageFinished();
    }
}