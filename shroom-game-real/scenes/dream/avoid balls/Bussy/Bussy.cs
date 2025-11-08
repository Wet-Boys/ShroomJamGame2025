using Godot;
using System;

public partial class Bussy : Node3D
{
    [Export] private RigidBody3D _rigidBody3D;
    [Export] private Area3D _area3D;
    private Vector3 _forward;

    public override void _Ready()
    {
        base._Ready();
        _area3D.BodyEntered += Area3DOnBodyEntered;
    }

    private void Area3DOnBodyEntered(Node body)
    {
        if (body.GetParent() is Ball ball)
        {
            GD.Print("death");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _forward = _rigidBody3D.GlobalTransform.Basis.Z;
        if (Input.IsActionPressed("movement.move_forward"))
        {
            MoveForward();
        }
        if (Input.IsActionPressed("movement.move_backward"))
        {
            MoveBack();
        }
        if (Input.IsActionPressed("movement.move_left"))
        {
            MoveLeft();
        }
        if (Input.IsActionPressed("movement.move_right"))
        {
            MoveRight();
        }
    }
    public void MoveLeft()
    {
        _rigidBody3D.RotateY(Mathf.DegToRad(3));
    }
    public void MoveRight()
    {
        _rigidBody3D.RotateY(Mathf.DegToRad(-3));
    }
    public void MoveBack()
    {
        _rigidBody3D.LinearVelocity -= _forward * .25f;
    }
    public void MoveForward()
    {
        _rigidBody3D.LinearVelocity += _forward * .25f;
    }
    
}
