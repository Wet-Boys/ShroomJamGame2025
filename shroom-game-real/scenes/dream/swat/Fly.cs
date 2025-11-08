using Godot;
using System;

public partial class Fly : Node3D
{
    [Export]
    private RigidBody3D _rigidBody3D;
    private RandomNumberGenerator _rng = new();
    [Export]
    private Timer _moveTimer;

    public static int flyCount = 0;
    public override void _Ready()
    {
        base._Ready();
        _rigidBody3D.SetGravityScale(0);
        _rigidBody3D.BodyEntered += RigidBody3DOnBodyEntered;
        MoveToNewSpot();
        _moveTimer.Timeout += MoveToNewSpot;
        flyCount++;
    }

    private void RigidBody3DOnBodyEntered(Node body)
    {
        MoveToNewSpot();
    }

    private void MoveToNewSpot()
    {
        Stop();
        _rigidBody3D.RotateY(Mathf.DegToRad(_rng.RandfRange(0,360)));
        var forward = _rigidBody3D.GlobalTransform.Basis.Z;
        _rigidBody3D.LinearVelocity += forward * 3;
    }

    private void Stop()
    {
        _rigidBody3D.LinearVelocity = Vector3.Zero;
        _moveTimer.Start(_rng.RandfRange(.75f, 1.25f));
    }
    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationPredelete)
        {
            flyCount--;
            GD.Print($"{Fly.flyCount} flies left");
        }
    }
}
