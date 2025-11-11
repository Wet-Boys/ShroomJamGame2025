using Godot;
using System;

public partial class PlayerVisualHandler : Node3D
{
    [Export] public Node3D visualRotationNode;
    [Export] public Node3D desiredVisualRotationNode;
    [Export] public AnimationTree animationTree;
    public CharacterBody3D player;
    private bool _inAir = false;
    private bool _walking = false;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (player.Velocity.Length() < .7f)
        {
            if (_walking)
            {
                _walking = false;
                animationTree.Set("parameters/Walking/request", (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
            }
        }
        else
        {
            if (!_walking)
            {
                _walking = true;
                animationTree.Set("parameters/Walking/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            }
        }

        if (player.IsOnFloor())
        {
            if (_inAir)
            {
                _inAir = false;
                animationTree.Set("parameters/Jumping/request", (int)AnimationNodeOneShot.OneShotRequest.FadeOut);
            }
        }
        else
        {
            if (!_inAir)
            {
                _inAir = true;
                animationTree.Set("parameters/Jumping/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            }
        }
    }

    public void RotateVisuals(double delta, Vector3 characterVelocity, bool snap)
    {
        characterVelocity -= new Vector3(0, characterVelocity.Y, 0);
        desiredVisualRotationNode.LookAt((desiredVisualRotationNode.GlobalPosition + characterVelocity) * 1.0001f);
        if (snap)
        {
            visualRotationNode.Rotation = new Vector3(visualRotationNode.Rotation.X, desiredVisualRotationNode.Rotation.Y, visualRotationNode.Rotation.Z);
        }
        else
        {
            visualRotationNode.Rotation = new Vector3(visualRotationNode.Rotation.X, Mathf.LerpAngle(visualRotationNode.Rotation.Y, desiredVisualRotationNode.Rotation.Y, (float)delta * 15), visualRotationNode.Rotation.Z);
        }
    }
}
