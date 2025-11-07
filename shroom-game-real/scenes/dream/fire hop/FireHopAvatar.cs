using Godot;
using System;

public partial class FireHopAvatar : Node3D
{
    [Export] private RigidBody3D _rigidBody3D;
    public int health = 3;
    public int score = 0;

    private bool Grounded => _rigidBody3D.Position.Y < -1.4f;

    public void MoveLeft()
    {
        if (Grounded)
            _rigidBody3D.SetLinearVelocity(new Vector3(-2, _rigidBody3D.LinearVelocity.Y, 0));
    }

    public void MoveRight()
    {
        if (Grounded)
            _rigidBody3D.SetLinearVelocity(new Vector3(2, _rigidBody3D.LinearVelocity.Y, 0));
    }

    public void Jump()
    {
        if (Grounded)
        {
            _rigidBody3D.SetLinearVelocity(new Vector3(_rigidBody3D.LinearVelocity.X, 4, 0));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Input.IsActionJustPressed("testJ"))
        {
            Jump();
        }
        if (Input.IsActionPressed("testL"))
        {
            MoveLeft();
        }
        if (Input.IsActionPressed("testR"))
        {
            MoveRight();
        }
    }
}
