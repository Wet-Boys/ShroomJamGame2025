using Godot;
using System;

public partial class Physicsball : RigidBody3D
{
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GlobalPosition.Y < -25)
        {
            QueueFree();
        }
    }
}
