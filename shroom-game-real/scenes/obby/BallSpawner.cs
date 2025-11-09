using Godot;
using System;

public partial class BallSpawner : MeshInstance3D
{
    private int _direction = -1;
    [Export]
    private Timer _spawnTimer;
    [Export]
    private PackedScene _ballPrefab;
    public override void _Ready()
    {
        base._Ready();
        _spawnTimer.Timeout += SpawnBall;
        _spawnTimer.Start(.1f);
    }

    private void SpawnBall()
    {
        Node3D ball = _ballPrefab.Instantiate<Node3D>();
        GetParent().AddChild(ball);
        ball.GlobalPosition = GlobalPosition - new Vector3(0, 2, 0);
        _spawnTimer.Start(.2f);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_direction == -1 && GlobalPosition.X < -9)
        {
            _direction = 1;
        }
        else if (_direction == 1 && GlobalPosition.X > 9)
        {
            _direction = -1;
        }

        GlobalPosition += new Vector3(_direction * (float)delta * 25, 0, 0);
    }
}
