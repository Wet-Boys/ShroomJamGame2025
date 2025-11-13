using Godot;
using ShroomGameReal.Interactables;
using ShroomGameReal.Player;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class FoodInteractable : InteractableStaticBody3D
{

    public override bool CanInteract => true;
    [Export] private CollisionShape3D _collisionShape3D;
    public override string GetInteractText()
    {
        return "Eat";
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!IsVisibleInTree())
        {
            _collisionShape3D.Position = new Vector3(100, 0, 0);
        }
        else
        {
            _collisionShape3D.Position = Vector3.Zero;
        }
    }

    protected override void Interact(PlayerController player)
    {
        GameFlowHandler.instance.FoodInteract(this);
    }
}