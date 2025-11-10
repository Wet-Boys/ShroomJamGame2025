using System;
using Godot;
using ShroomGameReal.Player;

namespace ShroomGameReal.Interactables;

[GlobalClass]
public partial class Interactor : RayCast3D
{
    private PlayerController _player;
    private InteractLabel _interactLabel;
    private IInteractable _currentInteractable;
    
    [Export]
    public bool Active { get; set; }

    public override void _Ready()
    {
        _player = GetOwner<PlayerController>();
        _interactLabel = GetNode<InteractLabel>("%Interact Label");

        UpdateLabelVisibility();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GlobalGameState.Instance.IsMainPaused)
            return;

        if (_currentInteractable is not null && !_currentInteractable.CanInteract)
        {
            _currentInteractable.OnDeselected();
            _currentInteractable = null;
            return;
        }
        
        if (!IsColliding() || !Active)
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
        
        if (!interactable.CanInteract)
            return;

        if (interactable != _currentInteractable)
            _interactLabel.SetText(interactable.GetInteractText());

        interactable.OnSelected();
        _currentInteractable = interactable;
    }

    public override void _Process(double delta)
    {
        UpdateLabelVisibility();

        if (_currentInteractable is null || !Active || GlobalGameState.Instance.IsMainPaused)
            return;

        var (screenBoundsMin, screenBoundsMax) = _currentInteractable.GetScreenBounds();
        var screenBoundsSize = screenBoundsMax - screenBoundsMin;
        var screenBoundsCenter = screenBoundsMin + screenBoundsSize / 2f;

        var windowSize = (Vector2)GetViewport().GetWindow().Size;
        var windowCenter = windowSize / 2f;
        
        var screenToWindow = windowCenter - screenBoundsCenter;
        var screenToWindowDir = screenToWindow.Normalized();

        var pointOnBounds = screenBoundsCenter + screenToWindowDir * screenBoundsSize;
        pointOnBounds.X = Mathf.Clamp(pointOnBounds.X, screenBoundsMin.X, screenBoundsMax.X);
        pointOnBounds.Y = Mathf.Clamp(pointOnBounds.Y, screenBoundsMin.Y, screenBoundsMax.Y);

        var labelSize = _interactLabel.Size;
        var labelOffset = screenToWindowDir * ((labelSize / 2f) + (Vector2.One * _interactLabel.padding));
        
        _interactLabel.SetLabelPosition(pointOnBounds + labelOffset);

        _interactLabel.SetLinePoints(_interactLabel.GlobalPosition, screenBoundsCenter);
    }

    private void UpdateLabelVisibility()
    {
        if (_currentInteractable is null)
        {
            _interactLabel.Visible = false;
            return;
        }
        
        _interactLabel.Visible = _currentInteractable.CanInteract && Active;
    }

    public override void _Input(InputEvent @event)
    {
        if (_currentInteractable is null || !Active || GlobalGameState.Instance.IsMainPaused)
            return;
        
        if (@event.IsActionPressed("interact"))
            _currentInteractable.OnInteract(_player);
    }
}