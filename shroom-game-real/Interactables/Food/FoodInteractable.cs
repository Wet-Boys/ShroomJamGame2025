using Godot;
using ShroomGameReal.Interactables;
using ShroomGameReal.Player;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class FoodInteractable : InteractableStaticBody3D
{

    public override bool CanInteract => true;
    public override string GetInteractText()
    {
        return "Eat";
    }

    protected override void Interact(PlayerController player)
    {
        GameFlowHandler.instance.FoodInteract(this);
    }
}