using Godot;
using System;
using ShroomGameReal.Player.PlayerStates;

public partial class PlayerVisualHandler : Node3D
{
    [Export] public Node3D visualRotationNode;
    [Export] public Node3D desiredVisualRotationNode;
    [Export] public AnimationTree animationTree;
    public CharacterBody3D player;
    private bool _inAir = false;
    private bool _walking = false;
    [Export] public BoneAttachment3D animationHeadLockNode;
    [Export] public Node3D animationHeadLockExtraNode;

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

    public void CoughingBaby()
    {
        LockHeadAnimation("Coughing");
    }

    private void UnlockHead(StringName animName)
    {
        UnlockBody();
        animationTree.AnimationFinished -= UnlockHead;
    }

    public void Succ()
    {
        LockHeadAnimation("FallIntoTv");
    }

    private void LockHeadAnimation(string  animName)
    {
        LockBody();
        animationTree.Set($"parameters/{animName}/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        animationTree.AnimationFinished += UnlockHead;
    }

    public void UnlockBody()
    {
        FirstPersonOverworldPlayerState.isInAnimation = false;
        animationHeadLockNode.OverridePose = true;
        animationHeadLockNode.Position = Vector3.Zero;
        animationHeadLockNode.Rotation = Vector3.Zero;
        animationHeadLockExtraNode.Position = Vector3.Zero;
        animationHeadLockExtraNode.Rotation = Vector3.Zero;
    }

    public void LockBody()
    {
        FirstPersonOverworldPlayerState.isInAnimation = true;
        animationHeadLockNode.OverridePose = false;
        animationHeadLockExtraNode.Position = new Vector3(0, 0.2f, 0);
        animationHeadLockExtraNode.RotationDegrees = new Vector3(0, 180, 0);
    }
}
