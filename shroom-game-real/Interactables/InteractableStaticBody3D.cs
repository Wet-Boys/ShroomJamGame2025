using Godot;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class InteractableStaticBody3D : StaticBody3D, IInteractable
{
    public virtual void OnSelected() { }

    public virtual void OnDeselected() { }

    public virtual void OnInteract() { }
}