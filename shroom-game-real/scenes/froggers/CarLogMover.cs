using Godot;
using System;
using ShroomGameReal;

public partial class CarLogMover : Node3D
{
    public float speed;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Mathf.Abs(Position.Z) > 20)
        {
            QueueFree();
        }
        Position += new Vector3(0, 0, speed * (float)delta) * GlobalGameState.Instance.GameTimeScale;
    }
}
