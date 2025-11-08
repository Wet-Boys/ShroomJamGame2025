using Godot;
using ShroomGameReal.scenes.Scoot_Shoot.Enemies;

namespace ShroomGameReal.scenes.Scoot_Shoot;

[GlobalClass]
public partial class ScootShootOnRailsGame : Node3D
{
    [Export]
    public float playerHealth = 100f;

    [Export]
    public float playerDamage = 50f;
    
    [Export]
    public ScootShootStage[] stages = [];

    [Signal]
    public delegate void OnGameFinishedEventHandler(bool isVictory);
    
    [Signal]
    public delegate void OnGameQuitEventHandler();
    
    public bool GameStarted { get; private set; }
    public bool GameOver { get; private set; }

    private uint _stagesFinished;

    private CrtScreenZapper _screenZapper;
    private Camera3D _camera;
    private RayCast3D _zapperRayCast;

    public override void _Ready()
    {
        _screenZapper = GetNode<CrtScreenZapper>("CrtScreenZapper");
        _camera = GetNode<Camera3D>("Camera3D");
        _zapperRayCast = GetNode<RayCast3D>("Zapper RayCast");
        
        foreach (var stage in stages)
            stage.OnStageFinished += CurrentStageFinished;
        
        StartGame();
    }

    public void StartGame()
    {
        stages[0].EnterStage();
        GameStarted = true;
    }

    public void DamagePlayer(float amount)
    {
        if (GameOver)
            return;
        
        playerHealth -= amount;
        if (playerHealth <= 0)
            OnGameLost();
        
        GD.Print($"Player took {amount} of  damage, remaining health: {playerHealth}");
    }

    private void CurrentStageFinished()
    {
        _stagesFinished++;
        if (_stagesFinished >= stages.Length)
        {
            OnGameWon();
        }
        else
        {
            stages[_stagesFinished].EnterStage();
        }
    }

    private void OnGameWon()
    {
        GameOver = true;
        EmitSignalOnGameFinished(true);
        GD.Print("Player won");
    }

    private void OnGameLost()
    {
        GameOver = true;
        EmitSignalOnGameFinished(false);
        GD.Print("Player lost");
    }

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();
        _zapperRayCast.GlobalPosition = _camera.ProjectRayOrigin(mousePos);
        _zapperRayCast.TargetPosition = _zapperRayCast.GlobalPosition + _camera.ProjectRayNormal(mousePos) * _screenZapper.maxZapperRange;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("primary_action"))
        {
            _zapperRayCast.ForceRaycastUpdate();
            if (_zapperRayCast.IsColliding())
            {
                var collision = _zapperRayCast.GetCollider();
                if (collision is ShooterEnemy enemy)
                {
                    if (enemy.stage.Started)
                    {
                        GD.Print("Hit Enemy");
                        enemy.HealthComponent.Damage(playerDamage);
                    }
                    else
                    {
                        GD.Print("Enemy isn't ready");
                    }
                }
            }
        }
    }
}