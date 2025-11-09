using Godot;
using ShroomGameReal.Player;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class InteractableRigidBody3D : RigidBody3D, IInteractable
{
    public bool CanInteract { get; }
    
    public virtual void OnSelected() { }

    public virtual void OnDeselected() { }

    public virtual void OnInteract(PlayerController player) { }
}