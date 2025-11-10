using Godot;
using System;
using ShroomGameReal.Utilities;

public partial class FlySwatter : Node3D
{
    [Export] private Camera3D _camera3D;
    [Export] private float _distance = 2;
    [Export] private AnimationTree _animationTree;
    [Export] private float _cooldown = .55f;
    private double _currentCooldown;
    [Export] private Timer _hitBoxTimer;
    [Export] private Area3D _area3D;
    private SwatGameState _gameState;

    public override void _Ready()
    {
        _hitBoxTimer.Timeout += HitBoxTimerOnTimeout;
        _area3D.BodyEntered += HitFly;
        _gameState = GetParent<SwatGameState>();
    }

    private void HitFly(Node3D body)
    {
        if (body.GetParent() is Fly fly)
        {
            fly.QueueFree();
        }
    }

    private void HitBoxTimerOnTimeout()
    {
        if (_area3D.Monitoring)
        {
            _area3D.Monitoring = false;
            _hitBoxTimer.Stop();
        }
        else
        {
            _area3D.Monitoring = true;
            _hitBoxTimer.Start(.15f);
        }
    }

    private Vector2 _mouse2dPosition;
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouse inputEvent)
        {
            _mouse2dPosition = inputEvent.Position;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_gameState.IsActive)
        {
            var mouse3dPosition = _camera3D.ProjectPosition(_mouse2dPosition, _distance);
            if (_currentCooldown > 0)
            {
                _currentCooldown -= delta;
                if (_currentCooldown < .15f)
                {
                    GlobalPosition = GlobalPosition.Lerp(mouse3dPosition, .15f);
                }
            }
            else
            {
                GlobalPosition = mouse3dPosition;
                LookAt(_camera3D.ProjectPosition(_mouse2dPosition, _distance + 1), Vector3.Forward);
                // Rotation += new Vector3(-90.0f, 0, 0);
                if (Input.IsActionPressed("primary_action"))
                {
                    _hitBoxTimer.Start(.25f);
                    _animationTree.Set($"parameters/Swat/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
                    _currentCooldown = _cooldown;
                }   
            }   
        }
    }
}
