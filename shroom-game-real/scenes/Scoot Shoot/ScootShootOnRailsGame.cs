using Godot;
using ShroomGameReal.scenes.Scoot_Shoot.Enemies;
using ShroomGameReal.scenes.Scoot_Shoot.Main_Menu;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Ui.PauseMenu;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.Scoot_Shoot;

[GlobalClass]
public partial class ScootShootOnRailsGame : BaseTvGameState
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
    private PauseMenuController _pauseMenu;
    public MainMenuController mainMenu;

    public override void _Ready()
    {
        _screenZapper = GetNode<CrtScreenZapper>("CrtScreenZapper");
        _camera = GetNode<Camera3D>("Camera3D");
        _zapperRayCast = GetNode<RayCast3D>("Zapper RayCast");
        _pauseMenu = GetNode<PauseMenuController>("%Pause Menu");
        
        foreach (var stage in stages)
            stage.OnStageFinished += CurrentStageFinished;

        _pauseMenu.baitQuitButton.Pressed += OnGameLost;

        CanActivate = true;
        infoText = "Shoot!";
    }

    public override void OnEnterState()
    {
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.ConfinedHidden);
    }

    public void StartGame()
    {
        stages[0].EnterStage();
        GameStarted = true;
    }

    public void PlaySingleStage(int stageNum)
    {
        mainMenu.Visible = false;
        _stagesFinished = (uint)stages.Length;
        stages[stageNum].EnterStage();
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

        CanActivate = false;
        
        ExitTv();
    }

    private void OnGameLost()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FailMinigame(this);
        }
        else
        {
            GameOver = true;
            EmitSignalOnGameFinished(false);
            GD.Print("Player lost");

            CanActivate = false;
        
            ExitTv();   
        }
    }

    public override void ExitTv()
    {
        if (_pauseMenu.Visible)
        {
            MouseReleaser.Instance.RequestLockedMouse();
        }
        
        _screenZapper.Visible = false;
        GlobalGameState.Instance.InBaitMode = false;
        GameOver = true;
        base.ExitTv();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("escape") && GameStarted && CanActivate)
        {
            if (_pauseMenu.Visible)
            {
                _pauseMenu.Resume();
            }
            else
            {
                _pauseMenu.Pause();
            }
        }

        if (TimeScale <= 0)
            return;
        
        if (@event is InputEventMouse mouseEvent)
        {
            _zapperRayCast.GlobalPosition = _camera.ProjectRayOrigin(mouseEvent.Position);
            _zapperRayCast.TargetPosition = _zapperRayCast.GlobalPosition + _camera.ProjectRayNormal(mouseEvent.Position) * _screenZapper.maxZapperRange;
            _zapperRayCast.ForceRaycastUpdate();
        }
        
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