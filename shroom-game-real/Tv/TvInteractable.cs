using Godot;
using ShroomGameReal.Interactables;
using ShroomGameReal.Player;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class TvInteractable : InteractableStaticBody3D
{
    public TvController TvController { get; private set; }

    public override bool CanInteract => TvController.CanActivateGameState;

    public override void _Ready()
    {
        base._Ready();
        TvController = GetNode<TvController>("TvController");
    }

    public override void OnInteract(PlayerController player)
    {
        if (!CanInteract)
            return;
        
        GD.Print("TV Interact Attempted");

        var tvGameState = player.AllPlayerStates.tvGame;

        tvGameState.tvController = TvController;
        
        player.CurrentState = tvGameState;
    }
}