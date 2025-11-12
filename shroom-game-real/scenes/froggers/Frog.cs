using Godot;
using System;
using ShroomGameReal;

public partial class Frog : Node3D
{
    [Export] private Area3D _hitbox;
    private Node3D _currentLog;
    private Vector3 _currentLogPrevPosition = Vector3.Zero;
    public Vector3 respawnPosition;
    private FroggerGameState _gameState;

    public override void _Ready()
    {
        base._Ready();
        _hitbox.AreaEntered += HitboxOnBodyEntered;
        _hitbox.AreaExited += HitboxOnBodyExited;
        respawnPosition = Position;
        _gameState = GetParent<FroggerGameState>();
    }

    private void HitboxOnBodyExited(Node3D body)
    {
        if (body.GetParent() == _currentLog)
        {
            _currentLog = null;
            CheckIfNoPlatformAfterFrame();
        }
    }

    private async void CheckIfNoPlatformAfterFrame()
    {
        await ToSignal(GetTree(), "physics_frame");
        await ToSignal(GetTree(), "physics_frame");
        if (Position.X > -6 && Position.X < 16 && _currentLog is null)
        {
            Respawn();
        }
    }

    private void HitboxOnBodyEntered(Node3D body)
    {
        if (body is Area3D { CollisionLayer: 1 })
        {
            _currentLog = body.GetParent<Node3D>();
            _currentLogPrevPosition = Vector3.Zero;
        }
        else
        {
            Respawn();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_gameState.IsActive && !GlobalGameState.Instance.IsMainPaused)
        {
            if (Input.IsActionJustPressed("movement.move_forward"))
            {
                Move(new Vector3(2, 0, 0));
            }
            if (Input.IsActionJustPressed("movement.move_backward"))
            {
                Move(new Vector3(-2, 0, 0));
            }
            if (Input.IsActionJustPressed("movement.move_right"))
            {
                Move(new Vector3(0, 0, 1));
            }
            if (Input.IsActionJustPressed("movement.move_left"))
            {
                Move(new Vector3(0, 0, -1));
            }   
        }



        if (_currentLog is not null)
        {
            if (_currentLogPrevPosition != Vector3.Zero)
            {
                GlobalPosition += _currentLog.Position - _currentLogPrevPosition;
            }
            _currentLogPrevPosition  = _currentLog.Position;
        }
        
        
        GlobalPosition = GlobalPosition.Clamp(new Vector3(-24, 2, -12) , new Vector3(16, 2, 12));
    }

    private void Move(Vector3 inputMovement)
    {
        GlobalPosition += inputMovement;
        if (Position.X > -6 && Position.X < 16)
        {
            CheckIfNoPlatformAfterFrame();
        }
    }

    public void Respawn()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.Lives--;
            _gameState.ExitTv();
            _gameState.CanActivate = false;
        }
        else
        {
            Position = respawnPosition;
        }
    }
}
