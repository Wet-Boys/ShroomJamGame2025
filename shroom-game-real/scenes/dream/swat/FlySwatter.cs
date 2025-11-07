using Godot;
using System;

public partial class FlySwatter : Node3D
{
    [Export] private Camera3D _camera3D;
    [Export] private float _distance = 2;
    [Export] private AnimationTree _animationTree;

    public override void _Process(double delta)
    {
        base._Process(delta);
        var mouse2dPosition = GetViewport().GetMousePosition();
        var mouse3dPosition = _camera3D.ProjectPosition(mouse2dPosition, _distance);
        GlobalPosition = mouse3dPosition + new Vector3(0,0,1);
        if (Input.IsActionPressed("primary_action"))
        {
            _animationTree.Set($"parameters/Swat/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }
}
