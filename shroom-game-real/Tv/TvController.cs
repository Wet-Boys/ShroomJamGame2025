using Godot;
using ShroomGameReal.Player;
using ShroomGameReal.Tv.GameStates;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class TvController : Node
{
    [Export]
    public Node3D cameraProxy;
    
    [Export]
    public Material screenViewportMaterial;
    
    [ExportGroup("Debug")]
    [Export]
    public PackedScene debugSceneToLoad;
    
    private MeshInstance3D _tvMeshInstance;
    private SubViewport _viewport;
    private MeshInstance3D _screenQuad;
    private Area3D _mouseArea;

    private bool _mouseInside;
    private Vector2 _lastScreenPos;
    private bool _lastScreenPosInitialized;
    private float _lastEventTime = -1f;
    private PlayerController _player;
    private Node3D _background;
    
    public bool HasSubWorld => GameState is not null;
    
    public bool CanActivateGameState => GameState is not null && GameState.CanActivate;

    private bool _isActive;
    public BaseTvGameState GameState { get; private set; }
    public static TvController instance;

    [Signal]
    public delegate void ExitTvEventHandler();

    public override void _Ready()
    {
        _tvMeshInstance = GetNode<MeshInstance3D>("../Model/Slavic Tv2/Slavic_Tv_Mesh");
        _viewport = GetNode<SubViewport>("%SubWorld");
        _mouseArea = GetNode<Area3D>("%Mouse Interaction Area");
        _screenQuad = GetNode<MeshInstance3D>("%Screen Quad");
        _background = GetNode<Node3D>("%Background");
        
        _mouseArea.MouseEntered += MouseEnteredScreen;
        _mouseArea.MouseExited += MouseExitedScreen;
        _mouseArea.InputEvent += MouseInputEvent;
        instance = this;
    }

    public override void _EnterTree()
    {
        RenderingServer.Singleton.FramePreDraw += RenderSubWorld;
    }

    public override void _ExitTree()
    {
        RenderingServer.Singleton.FramePreDraw -= RenderSubWorld;
    }

    public void EnterTvState(PlayerController player)
    {
        _background.Scale = Vector3.One;
        
        _player = player;
        _isActive = true;

        GameState.OnEnterState();
    }

    public void ExitTvState()
    {
        _background.Scale = Vector3.Zero;
        
        EmitSignalExitTv();
        _isActive = false;
    }
    
    private void MouseEnteredScreen()
    {
        _mouseInside = true;
        GD.Print("Mouse Entered");
    }

    private void MouseExitedScreen()
    {
        _mouseInside = false;
        GD.Print("Mouse Exited");
    }

    private void MouseInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (!_isActive)
            return;
        
        var screenSize = ((QuadMesh)_screenQuad.Mesh).Size;

        var now = Time.Singleton.GetTicksMsec() / 1000.0f;

        var eventPos3D = _screenQuad.GlobalTransform.AffineInverse() * eventPosition;
        var eventPos2D = Vector2.Zero;

        if (_mouseInside)
        {
            eventPos2D = new Vector2(eventPos3D.X, -eventPos3D.Y);

            eventPos2D.X /= screenSize.X;
            eventPos2D.Y /= screenSize.Y;

            eventPos2D += Vector2.One * 0.5f;

            eventPos2D *= _viewport.Size;
        }
        else if (_lastScreenPosInitialized)
        {
            eventPos2D = _lastScreenPos;
        }

        if (@event is InputEventMouse mouseEvent)
        {
            mouseEvent.Position = eventPos2D;
            mouseEvent.GlobalPosition = eventPos2D;

            if (mouseEvent is InputEventMouseMotion mouseMotion)
            {
                if (!_lastScreenPosInitialized)
                {
                    mouseMotion.Relative = Vector2.Zero;
                }
                else
                {
                    mouseMotion.Relative = eventPos2D - _lastScreenPos;
                    mouseMotion.Velocity = mouseMotion.Relative / (now - _lastEventTime);
                }
            }
        }

        _lastScreenPos = eventPos2D;
        _lastScreenPosInitialized = true;
        _lastEventTime = now;

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            _viewport.PushInput(@event);
        }
    }

    public Node SetTvSubWorld(PackedScene scene)
    {
        ClearTvSubWorld();
        var instance = scene.Instantiate();
        _viewport.AddChild(instance);

        GameState = (BaseTvGameState)instance;
        GameState.OnExitTv += OnGameStateExit;
        
        UpdateViewport();
        return instance;
    }

    private void OnGameStateExit()
    {
        var nextState = _player.AllPlayerStates.firstPersonOverworld;
        _player.CurrentState = nextState;
    }

    public void ClearTvSubWorld()
    {
        foreach (var child in _viewport.GetChildren())
            child.QueueFree();
    }

    private void UpdateViewport()
    {
        var material = _tvMeshInstance.GetSurfaceOverrideMaterial(0);
        if (material is null)
        {
            material = (Material)screenViewportMaterial.Duplicate();
            _tvMeshInstance.SetSurfaceOverrideMaterial(0, material);
        }
        material.Set("shader_parameter/viewportTexture", _viewport.GetTexture());
    }
    
    private void RenderSubWorld()
    {
        if (!HasSubWorld)
            return;

        if (!IsInsideTree())
            return;
        
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { PhysicalKeycode: Key.H, Pressed: true })
        {
            SetTvSubWorld(debugSceneToLoad);
        }

        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _viewport.PushInput(@event);
            return;
        }

        if (@event is InputEventMouse || !_isActive)
            return;
        
        _viewport.PushInput(@event);
    }
}