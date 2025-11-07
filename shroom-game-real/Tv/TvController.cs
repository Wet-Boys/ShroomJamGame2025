using Godot;

namespace ShroomGameReal.Tv;

[GlobalClass]
public partial class TvController : Node3D
{
    [Export]
    public Material screenViewportMaterial;
    
    [ExportGroup("Debug")]
    [Export]
    public PackedScene debugSceneToLoad;
    
    private MeshInstance3D _tvMeshInstance;
    private SubViewport _viewport;
    
    private bool HasSubWorld => _viewport.GetChildCount() > 0;

    public override void _Ready()
    {
        _tvMeshInstance = GetNode<MeshInstance3D>("Model/Cube");
        _viewport = GetNode<SubViewport>("SubWorld");
    }

    public override void _EnterTree()
    {
        RenderingServer.Singleton.FramePreDraw += RenderSubWorld;
    }

    public override void _ExitTree()
    {
        RenderingServer.Singleton.FramePreDraw -= RenderSubWorld;
    }

    public void SetTvSubWorld(PackedScene scene)
    {
        ClearTvSubWorld();
        
        var instance = scene.Instantiate();
        _viewport.AddChild(instance);
        
        UpdateViewport();
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

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { PhysicalKeycode: Key.H, Pressed: true })
        {
            SetTvSubWorld(debugSceneToLoad);
        }
    }
}