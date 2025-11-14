using Godot;
using System;
using ShroomGameReal;
using ShroomGameReal.scenes.obby;
using FloatSettingsEntry = ShroomGameReal.Utilities.Settings.SettingsEntries.FloatSettingsEntry;
using SettingsManager = ShroomGameReal.Utilities.Settings.SettingsManager;

public partial class ObbyPlayer : Node3D
{
    [Export] public Node3D cameraContainer;
    [Export] public Node3D cameraX;
    [Export] public Node3D cameraY;
    [Export] public Node3D cameraPosition;
    [Export] public Camera3D camera;
    [Export] public ShapeCast3D cameraRay;
    [Export] public RigidBody3D ragdollBody;
    [Export] public CharacterBody3D characterBody;
    [Export] public Area3D hurtBox;
    private Vector3 _respawnPoint;
    private double _ragdollTimer = 0;
    private float _groundFriction = 10f;
    private float _airFriction = 9.5f;
    private Vector3 PlayerForward => characterBody!.GlobalBasis.X.Cross(Vector3.Up);
    private Vector3 PlayerRight => characterBody!.GlobalBasis.X;
    
    private FloatSettingsEntry _verticalSensitivity;
    private FloatSettingsEntry _horizontalSensitivity;
    private Vector2 MouseSensitivity => new(_horizontalSensitivity.Value, _verticalSensitivity.Value);
    private float _gravity = ProjectSettings.Singleton.GetSetting("physics/3d/default_gravity").AsSingle();
    private ObbyGameState _gameState;
    [Export] public PlayerVisualHandler visualHandler;
    
    [Export] public AudioStreamPlayer spawnSfx;
    [Export] public AudioStreamPlayer spawn10;
    [Export] public AudioStreamPlayer spawn20;
    [Export] public AudioStreamPlayer spawn7;
    [Export] public AudioStreamPlayer hitSfx;
    [Export] public AudioStreamPlayer deathSfx;
    [Export] public AudioStreamPlayer idleSfx;
    [Export] private AudioStreamPlayer _audioStreamPlayer;
    private int _currentRound = 1;
    private double _nothingEverHappensTimer = 0;
    private bool _needToRespawn = false;
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _verticalSensitivity = SettingsManager.Gameplay.Mouse.VerticalSensitivity;
        _horizontalSensitivity = SettingsManager.Gameplay.Mouse.HorizontalSensitivity;
        ragdollBody.Freeze = true;
        ragdollBody.AddCollisionExceptionWith(characterBody);
        characterBody.AddCollisionExceptionWith(ragdollBody);
        hurtBox.BodyEntered += HurtBoxOnBodyEntered;
        _respawnPoint = GlobalPosition;
        _gameState = GetParent().GetParent<ObbyGameState>();
        visualHandler.player = characterBody;
        cameraY.RotationDegrees = new Vector3(0, 180, 0);
        spawnSfx.Play();
        deathSfx.Finished += DeathSfxFinished;
        SpinObstacle.spinSpeed = 1;
    }

    private void HurtBoxOnBodyEntered(Node3D body)
    {
        if (body is VictoryBlock)
        {
            if (!ragdollBody.Freeze)
            {
                GD.Print("Tumble win?");
            }
            else
            {
                GD.Print("YOU WIN!");
                _gameState.FinishLevel();
            }
        }
        else if (body is BigRedBall)
        {
            _audioStreamPlayer.Play();
            _ragdollTimer = 3.8f;
            if (ragdollBody.Freeze)
            {
                BeginRagdoll();
                var launchDirection = ragdollBody.GlobalPosition.DirectionTo(body.GlobalPosition) * -13;
                ragdollBody.LinearVelocity += launchDirection;
            }   
        }
        else
        {
            _audioStreamPlayer.Play();
            if (ragdollBody.Freeze)
            {
                _ragdollTimer = 0;
                BeginRagdoll();
                if (body is ObbyHitbox hitbox)
                {
                    var launchDirection = hitbox.Velocity * 50;
                    // launchDirection *= ragdollBody.GlobalPosition.DirectionTo(hitbox.GlobalPosition);
                    ragdollBody.LinearVelocity += launchDirection;
                }
            }   
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (_gameState.IsActive && @event is InputEventMouseMotion motionEvent)
        {
            cameraY.RotateY(motionEvent.Relative.X * MouseSensitivity.X * -1 * .0005f);
            cameraX.RotateX(motionEvent.Relative.Y * MouseSensitivity.Y * .0005f);
        }
    }

    public override void _Process(double delta)
    {
        if (GlobalGameState.Instance.IsMainPaused)
            return;
        
        base._Process(delta);
        cameraX.Rotation =
            new Vector3(Mathf.Clamp(cameraX.Rotation.X, float.DegreesToRadians(-89f), float.DegreesToRadians((89f))),
                cameraX.Rotation.Y, cameraX.Rotation.Z);
        CameraRayCast();

        if (!ragdollBody.Freeze)
        {
            characterBody.Velocity = ragdollBody.LinearVelocity;
            characterBody.GlobalPosition = ragdollBody.GlobalPosition;
            characterBody.GlobalRotation = ragdollBody.GlobalRotation;
            _ragdollTimer += delta;
            if (_ragdollTimer > 4 && characterBody.GlobalPosition.Y >= 0)
            {
                EndRagdoll();
            }
        }

        if (characterBody.GlobalPosition.Y < 0 && !_needToRespawn)
        {
            Respawn();
        }
        cameraContainer.GlobalRotation = Vector3.Zero;
        
        visualHandler.RotateVisuals(delta, characterBody.Velocity, false);
        _nothingEverHappensTimer += delta;
        if (_nothingEverHappensTimer > 60)
        {
            idleSfx.Play();
        }
    }

    private void Respawn()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(_gameState, true);
        }
        else
        {
            _needToRespawn = true;
            idleSfx.Stop();
            hitSfx.Stop();
            deathSfx.Play();
            if (SpinObstacle.spinSpeed > .5f)
            {
                SpinObstacle.spinSpeed -= .025f;
            }
        }
    }
    private void DeathSfxFinished()
    {
        _currentRound++;
        EndRagdoll();
        characterBody.GlobalPosition = _respawnPoint;
        if (_currentRound == 7)
        {
            spawn7.Play();
        }
        else if (_currentRound == 10)
        {
            spawn10.Play();
        }
        else if (_currentRound == 20)
        {
            spawn20.Play();
        }
        else
        {
            spawnSfx.Play();
        }

        _needToRespawn = false;
    }
    private void CameraRayCast()
    {
        cameraPosition.Position = cameraRay.GetClosestCollisionSafeFraction() * cameraRay.TargetPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GlobalGameState.Instance.IsMainPaused)
            return;
        
        base._PhysicsProcess(delta);
        if (!ragdollBody.Freeze)
        {
            return;
        }
        var friction = characterBody.IsOnFloor() ? _groundFriction : _airFriction;
        characterBody.Velocity -= new Vector3(characterBody.Velocity.X, 0, characterBody.Velocity.Z) * friction * (float)delta * GlobalGameState.Instance.GameTimeScale;

        Vector2 test = Input.GetVector("movement.move_left", "movement.move_right", "movement.move_forward", "movement.move_backward");
        test *= _gameState.IsActive ? 1 : 0;
        Vector3 inputDirection = ((cameraY.GlobalBasis.X.Cross(Vector3.Up) * -test.Y) +
                                  (cameraY.GlobalBasis.Z.Cross(Vector3.Up) * test.X)).Normalized();
        var inputMovement = Vector2.Zero;
        if (ragdollBody.Freeze)
        {
            inputMovement = new Vector2(inputDirection.X, inputDirection.Z).Normalized();
        }
        
        characterBody.Velocity += PlayerForward * inputMovement.Y * GlobalGameState.Instance.GameTimeScale;
        characterBody.Velocity += PlayerRight * inputMovement.X * GlobalGameState.Instance.GameTimeScale;
        if (!characterBody.IsOnFloor())
        {
            characterBody.Velocity += Vector3.Down * _gravity * (float)delta * 3 * GlobalGameState.Instance.GameTimeScale;
        }
        else if (_gameState.IsActive && Input.IsActionJustPressed("movement.jump"))
        {
            characterBody.Velocity = new Vector3(characterBody.Velocity.X, 12, characterBody.Velocity.Z);
        }
        characterBody.MoveAndSlide();
    }

    public void BeginRagdoll()
    {
        if (ragdollBody.Freeze)
        {
            _nothingEverHappensTimer = 0;
            if (_ragdollTimer < 1)
            {
                hitSfx.Play();
            }
            idleSfx.Stop();
            ragdollBody.GlobalPosition = characterBody.GlobalPosition;
            ragdollBody.GlobalRotation = characterBody.GlobalRotation;
            ragdollBody.Freeze = false;
            ragdollBody.LinearVelocity = characterBody.Velocity;
            characterBody.CollisionLayer = 0;
            characterBody.CollisionMask = 0;
            characterBody.AxisLockAngularX = false;
            characterBody.AxisLockAngularZ = false;   
        }
    }

    public void EndRagdoll()
    {
        if (!ragdollBody.Freeze)
        {
            _nothingEverHappensTimer = 0;
            ragdollBody.Freeze = true;
            characterBody.CollisionLayer = 8;
            characterBody.CollisionMask = 33;
            characterBody.GlobalRotation = Vector3.Zero;
            characterBody.AxisLockAngularX = true;
            characterBody.AxisLockAngularZ = true;
            _ragdollTimer = 0;   
        }
    }
    
}
