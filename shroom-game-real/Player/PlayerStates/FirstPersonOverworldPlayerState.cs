using Godot;
using SettingsHelper;
using SettingsHelper.SettingsEntries;
using ShroomGameReal.Interactables;
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
    [Export(PropertyHint.Range, "0, 100,or_greater")]
    public float mass = 1f;
    
    private FloatSettingsEntry _verticalSensitivity;
    private FloatSettingsEntry _horizontalSensitivity;

    private Interactor _interactor;
    
    private Vector2 MouseSensitivity => new(_horizontalSensitivity.Value, _verticalSensitivity.Value);
    
    private Vector2 _mouseDelta;
    private float _gravity;
    public static bool isInAnimation;

    public override void _Ready()
    {
        base._Ready();
        
        _verticalSensitivity = SettingsManager.Gameplay.Mouse.VerticalSensitivity;
        _horizontalSensitivity = SettingsManager.Gameplay.Mouse.HorizontalSensitivity;
        
        _interactor = GetNode<Interactor>("%Interactor");
        
        _gravity = ProjectSettings.Singleton.GetSetting("physics/3d/default_gravity").AsSingle();
    }

    protected override void OnEnterState()
    {
        // Reset velocity on enter
        Player.Velocity = Vector3.Zero;
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Captured);
        
        SettingsManager.Camera.Fov.OnValueChanged += OnFovSettingChanged;
        Camera.TweenToFov(SettingsManager.Camera.Fov.Value, onEnterFovTweenDuration);

        _interactor.Active = true;
    }

    protected override void OnExitState()
    {
        SettingsManager.Camera.Fov.OnValueChanged -= OnFovSettingChanged;
        Camera.StopFovTween();

        _interactor.Active = false;
    }

    private void OnFovSettingChanged(float newFov)
    {
        Camera.Fov = newFov;
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsActive || GlobalGameState.Instance.IsMainPaused || isInAnimation)
            return;
        
        if (@event is InputEventMouseMotion mouseMotion)
            _mouseDelta += mouseMotion.ScreenRelative;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsActive)
            return;
        
        if (GlobalGameState.Instance.IsMainPaused)
            return;

        if (isInAnimation)
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
        
        // Apply gravity if not on floor
        if (!Player.IsOnFloor())
            Player.Velocity += Vector3.Down * _gravity * mass;
        // else if (Input.IsActionJustPressed("movement.jump"))
        //     Player.Velocity += Vector3.Up * 60;
        Player.MoveAndSlide();

        // Apply friction only the horizontal velocity while copying over the vertical velocity
        Player.Velocity = ((Player.Velocity * (Vector3.One - Vector3.Up)) * groundFriction) + (Player.Velocity * Vector3.Up);
    }
}