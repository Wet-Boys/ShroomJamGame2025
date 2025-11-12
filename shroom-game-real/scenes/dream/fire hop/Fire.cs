using Godot;
using System;
using ShroomGameReal;

public partial class Fire : Node3D
{
    [Export] private RigidBody3D _rigidBody;
    [Export] private Area3D _area;
    private float _moveDirection = 0;
    private bool _hasBeenScored = false;

    public override void _Ready()
    {
        _rigidBody.BodyEntered += RigidBodyOnBodyEntered;
        _area.BodyEntered += AreaOnBodyEntered;
    }

    private void AreaOnBodyEntered(Node3D body)
    {
        if (!_hasBeenScored && body.GetParent() is FireHopAvatar player)
        {
            _hasBeenScored = true;
            player.score++;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _rigidBody.SetLinearVelocity(new Vector3(_moveDirection, _rigidBody.LinearVelocity.Y, _rigidBody.LinearVelocity.Z));
        if (Mathf.Abs(Position.X) > 6)
        {
            QueueFree();
        }
    }

    private void RigidBodyOnBodyEntered(Node body)
    {
        if (body.Name == "Floor")
        {
            _rigidBody.SetGravityScale(0);
            _moveDirection = GlobalPosition.X > 0 ? -1 : 1;
        }
        else if (body.GetParent() is FireHopAvatar player)
        {
            player.health--;
            QueueFree();
            
            GameFlowHandler.instance.FailMinigame(player.gameState);
        }
    }
}
