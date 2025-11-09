using Godot;
using System;

public partial class ObbyHitbox : StaticBody3D
{
    public Vector3 prevPosition;

    public Vector3 Velocity
    {
        get
        {
            if (_hitboxToTakeVelocityFrom is not null)
            {
                return _hitboxToTakeVelocityFrom.Velocity;
            }
            return field;
        }
        set => field = value;
    }
    [Export] private ObbyHitbox _hitboxToTakeVelocityFrom;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = GlobalPosition - prevPosition;
        prevPosition = GlobalPosition;
    }
}
