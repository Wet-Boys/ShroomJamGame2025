using Godot;
using ShroomGameReal.Player.PlayerStates;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Player;

[GlobalClass]
public partial class PlayerController : CharacterBody3D
{
    [Export]
    public BasePlayerState initialState;

    [Export]
    public BasePlayerState CurrentState
    {
        get;
        set
        {
            field?.ExitState();
            field = value;
            field.EnterState();
        }
    }

    // Contains all possible players states.
    public PlayerStateList AllPlayerStates
    {
        get
        {
            field ??= GetNode<PlayerStateList>("PlayerStates");
            return field;
        }
    }
    
    public override void _Ready()
    {
        CurrentState = initialState;
    }

    public override void _Input(InputEvent @event)
    {
        // if (@event.IsActionPressed("escape"))
        //     ToggleMouse();
    }

    // TODO: Replace this with settings menu
    private void ToggleMouse()
    {
        if (MouseReleaser.Instance.IsMouseLocked)
        {
            MouseReleaser.Instance.RequestFreeMouse();
        }
        else
        {
            MouseReleaser.Instance.RequestLockedMouse();
        }
    }
}