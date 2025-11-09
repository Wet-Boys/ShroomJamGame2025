using Godot;
using ShroomGameReal.Player;

namespace ShroomGameReal.Interactables;

public interface IInteractable
{
    public bool CanInteract { get; }

    /// <summary>
    /// Called when currently selected by Interactor Ray
    /// </summary>
    public void OnSelected();

    /// <summary>
    /// Called when no longer selected by Interactor Ray
    /// </summary>
    public void OnDeselected();

    /// <summary>
    /// Called when currently selected by Interactor Ray and the "Interact" action is pressed
    /// </summary>
    /// <param name="player"></param>
    public void OnInteract(PlayerController player);

    public string GetInteractText();
    
    public (Vector2 screenMin, Vector2 screenMax) GetScreenBounds();
}