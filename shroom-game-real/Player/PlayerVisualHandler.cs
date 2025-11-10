using Godot;
using System;

public partial class PlayerVisualHandler : Node3D
{
    [Export] public Node3D visualRotationNode;
    [Export] public Node3D desiredVisualRotationNode;
    public void RotateVisuals(double delta, Vector3 characterVelocity, bool snap)
    {
        characterVelocity -= new Vector3(0, characterVelocity.Y, 0);
        if (characterVelocity.Length() > .7f)
        {
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
}
