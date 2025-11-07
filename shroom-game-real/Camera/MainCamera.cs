using Godot;

namespace ShroomGameReal.Camera;

[GlobalClass]
public partial class MainCamera : Camera3D
{
    public static MainCamera Instance { get; private set; }
    
    private Tween _tween;

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
}