using Godot;
using System;

public partial class Ball : Node3D
{
    [Export] private RigidBody3D _rigidBody3D;
    public override void _Ready()
    {
        base._Ready();
        RandomNumberGenerator rand = new RandomNumberGenerator();
        _rigidBody3D.LinearVelocity += new Vector3(rand.RandfRange(-10, 10), 0, rand.RandfRange(-10,10));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _rigidBody3D.LinearVelocity = _rigidBody3D.LinearVelocity.Normalized() * 3;
    }
}
