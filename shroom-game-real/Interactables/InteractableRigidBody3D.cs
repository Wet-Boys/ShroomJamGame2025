using Godot;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class InteractableRigidBody3D : RigidBody3D, IInteractable
{
    public virtual void OnSelected() { }

    public virtual void OnDeselected() { }

    public virtual void OnInteract() { }
}