using Godot;
using SettingsHelper;
using SettingsHelper.SettingsEntries;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.Player.PlayerStates;

[GlobalClass]
public partial class FirstPersonOverworldPlayerState : BasePlayerState
{
    [Export]
    public double onEnterFovTweenDuration = 1.0;
    
    [ExportGroup("Head")]
    [Export(PropertyHint.Range, "0,90,radians_as_degrees")]
    public float horizontalHeadRotationRange = Mathf.DegToRad(45f);
    [Export(PropertyHint.Range, "0,90,degrees")]
    public float maxVerticalHeadDegrees = 85f;
    [Export(PropertyHint.Range, "-90,0,degrees")]
    public float minVerticalHeadDegrees = -85f;
    
    [ExportGroup("Physics")]
    [Export]
    public float groundFriction = 0.85f;
    [Export]
    public float movementSpeed = 450f;
    
    private FloatSettingsEntry _verticalSensitivity;
    private FloatSettingsEntry _horizontalSensitivity;
    private Vector2 MouseSensitivity => new(_horizontalSensitivity.Value, _verticalSensitivity.Value);
    
    private Vector2 _mouseDelta;

    public override void _Ready()
    {
        base._Ready();
        
        _verticalSensitivity = SettingsManager.Gameplay.Mouse.VerticalSensitivity;
        _horizontalSensitivity = SettingsManager.Gameplay.Mouse.HorizontalSensitivity;
    }

    protected override void OnEnterState()
    {
        // Reset velocity on enter
        Player.Velocity = Vector3.Zero;
        
        SettingsManager.Camera.Fov.OnValueChanged += OnFovSettingChanged;
        Camera.TweenToFov(SettingsManager.Camera.Fov.Value, onEnterFovTweenDuration);
    }

    protected override void OnExitState()
    {
        SettingsManager.Camera.Fov.OnValueChanged -= OnFovSettingChanged;
    }

    private void OnFovSettingChanged(float newFov)
    {
        Camera.Fov = newFov;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
            _mouseDelta += mouseMotion.ScreenRelative;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsActive)
            return;
        
        var movementVector = Input.GetVector("movement.move_left", "movement.move_right", "movement.move_forward", "movement.move_backward");

        // Head/Camera Look handling
        var lookVelocity = _mouseDelta * MouseSensitivity * (float)delta;
        _mouseDelta = Vector2.Zero;

        // Only do head movement if mouse is locked
        if (MouseReleaser.Instance.IsMouseLocked)
        {
            var rotation = Head.RotationDegrees;
            rotation.X -= lookVelocity.Y;
            rotation.X = Mathf.Clamp(rotation.X, minVerticalHeadDegrees, maxVerticalHeadDegrees);
            
            rotation.Y -= lookVelocity.X;
            // TODO Make body rotate when you move

            var bodyRotation = Player.RotationDegrees;

            var bodyAngleToHead = (bodyRotation.Y * Vector3.Up).AngleTo(rotation.Y * Vector3.Up);
            if (Mathf.Abs(bodyAngleToHead) > horizontalHeadRotationRange)
            {
                // TODO
            }

            rotation.Z = 0;
            
            Head.RotationDegrees = rotation;
        }

        // Player Movement handling

        var right = Head.GlobalBasis.X;
        var forward = right.Cross(Vector3.Up);
        
        Player.Velocity += forward * movementVector.Y * movementSpeed;
        Player.Velocity += right * movementVector.X * movementSpeed;

        Player.MoveAndSlide();

        Player.Velocity *= groundFriction;
    }
}