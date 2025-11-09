using Godot;
using System;

public partial class SpinObstacle : Node3D
{
    [Export] private Node3D _partToSpin;
    [Export] private Vector3 _spinAmount;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _partToSpin.Rotation += _spinAmount;
    }
}
