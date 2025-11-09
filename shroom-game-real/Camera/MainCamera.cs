using Godot;

namespace ShroomGameReal.Camera;

[GlobalClass]
public partial class MainCamera : Camera3D
{
    public static MainCamera Instance { get; private set; }
    
    private Tween _tween;
    
    private bool _hasTransformOverride;
    private Transform3D _originalLocalTransform;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void TweenToFov(float newFov, double duration)
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "fov", newFov, duration);
    }

    public void SetPlayerModelVisibility(bool visible)
    {
        SetCullMaskValue(2, visible);
    }
    
    public void SetGlobalTransformOverride(Transform3D transformOverride)
    {
        if (!_hasTransformOverride)
            _originalLocalTransform = Transform;
        
        GlobalTransform = transformOverride;
        _hasTransformOverride = true;
    }
    
    public void ClearGlobalTransformOverride()
    {
        if (_hasTransformOverride)
            Transform = _originalLocalTransform;
        
        _hasTransformOverride = false;
    }
}