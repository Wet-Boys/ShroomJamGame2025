using Godot;
using ShroomGameReal.Camera;

namespace ShroomGameReal.Player.PlayerStates;

[GlobalClass]
public abstract partial class BasePlayerState : Node
{
    protected PlayerController Player;
    protected Node3D Head;
    protected MainCamera Camera;
    
    /// <summary>
    /// True when this state is the current state of the PlayerController.
    /// </summary>
    public bool IsActive { get; private set; }

    public override void _Ready()
    {
        Player = GetNode<PlayerController>("../../../Player");
        Head = Player.GetNode<Node3D>("Head");
        Camera = Head.GetNode<MainCamera>("Eyes/BoneAttachment/AnimationRotate/Camera");
    }

    public void EnterState()
    {
        IsActive = true;
        OnEnterState();
    }

    public void ExitState()
    {
        IsActive = false;
        OnExitState();
    }

    /// <summary>
    /// Runs when the Player enters this state
    /// </summary>
    protected abstract void OnEnterState();

    /// <summary>
    /// Runs when the Player exists this state
    /// </summary>
    protected abstract void OnExitState();
}