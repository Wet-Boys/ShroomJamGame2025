using Godot;
using System;
using ShroomGameReal;

public partial class SpinObstacle : Node3D
{
    [Export] private Node3D _partToSpin;
    [Export] private Vector3 _spinAmount;
    public static float spinSpeed = 1;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _partToSpin.Rotation += _spinAmount * GlobalGameState.Instance.GameTimeScale * spinSpeed;
    }
}
