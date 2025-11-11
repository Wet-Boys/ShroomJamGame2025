using Godot;
using System;

public partial class FireSpawner : Node3D
{
    [Export]
    private PackedScene _firePrefab;
    [Export]
    private Timer _spawnTimer;
    [Export] private float _fireInterval;
    private RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _spawnTimer.Timeout += SpawnFire;
        _spawnTimer.Start(_rng.RandfRange(0f, .5f));
    }

    private void SpawnFire()
    {
        _spawnTimer.Start(_fireInterval + _rng.RandfRange(.5f, 2f));
        Node3D newFire = _firePrefab.Instantiate<Node3D>();
        GetParent().AddChild(newFire);
        newFire.Position = Position + new Vector3(_rng.RandfRange(-.5f, .5f),-4,0);
    }
}
