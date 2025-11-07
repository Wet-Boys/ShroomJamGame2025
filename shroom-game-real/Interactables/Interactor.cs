using Godot;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class Interactor : RayCast3D
{
    private IInteractable _currentInteractable;

    public override void _PhysicsProcess(double delta)
    {
        if (!IsColliding())
        {
            if (_currentInteractable is not null && IsInstanceValid(_currentInteractable as GodotObject))
            {
                _currentInteractable.OnDeselected();
                _currentInteractable = null;
            }
            
            return;
        }

        var collider = GetCollider();
        if (collider is not IInteractable interactable)
            return;

        interactable.OnSelected();
        _currentInteractable = interactable;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
            _currentInteractable.OnInteract();
    }
}