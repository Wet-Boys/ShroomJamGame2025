using Godot;
using ShroomGameReal.Interactables;
using ShroomGameReal.Player;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class TvInteractable : InteractableStaticBody3D
{
    public TvController TvController { get; private set; }

    public override bool CanInteract => TvController.CanActivateGameState;
    public static TvInteractable instance;

    public override void _Ready()
    {
        base._Ready();
        TvController = GetNode<TvController>("TvController");
        instance = this;
    }

    protected override void Interact(PlayerController player)
    {
        GD.Print("TV Interact Attempted");

        var tvGameState = player.AllPlayerStates.tvGame;

        tvGameState.tvController = TvController;
        
        player.CurrentState = tvGameState;
    }
}