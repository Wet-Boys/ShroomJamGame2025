using Godot;
using ShroomGameReal.Tv;

namespace ShroomGameReal.Player.PlayerStates;

[GlobalClass]
public partial class TvGamePlayerState : BasePlayerState
{
    [Export]
    public float fov = 35f;
    
    [ExportGroup("Fog")]
    [Export]
    public FogMaterial backgroundFogMaterial;
    
    [Export]
    public float enterDensity = 3f;
    [Export]
    public float exitDensity;
    [Export]
    public double enterDuration = 0.5;
    [Export]
    public double exitDuration = 0.5;

    public TvController tvController;
    
    private Tween _tween;
    
    protected override void OnEnterState()
    {
        Camera.SetPlayerModelVisibility(false);
        Camera.SetGlobalTransformOverride(tvController.cameraProxy.GlobalTransform);
        Camera.Fov = fov;
        
        _tween?.Kill();
        _tween = CreateTween();

        _tween.TweenProperty(backgroundFogMaterial, "density", enterDensity, enterDuration)
            .SetEase(Tween.EaseType.In);
        
        tvController.EnterTvState(Player);
    }

    protected override void OnExitState()
    {
        _tween?.Kill();
        _tween = CreateTween();
        
        _tween.TweenProperty(backgroundFogMaterial, "density", exitDensity, exitDuration)
            .SetEase(Tween.EaseType.Out);
        
        Camera.ClearGlobalTransformOverride();
        Camera.SetPlayerModelVisibility(true);
        
        tvController.ExitTvState();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact") && IsActive)
        {
            tvController.GameState.ExitTv();
            GetViewport().SetInputAsHandled();
            // tvController.ExitTvState();
        }
    }
}