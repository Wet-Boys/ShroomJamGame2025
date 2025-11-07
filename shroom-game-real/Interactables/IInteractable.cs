namespace ShroomGameReal.Interactables;

public interface IInteractable
{
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
    public void OnInteract();
}