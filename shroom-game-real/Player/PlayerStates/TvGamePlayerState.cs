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
    public ShaderMaterial backgroundFogMaterial;
    
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
        _tween = CreateTween().SetParallel();

        _tween.TweenProperty(backgroundFogMaterial, "shader_parameter/blackout_mix", enterDensity, enterDuration);
        
        tvController.EnterTvState(Player);
        Player.visualHandler.Visible = false;
    }

    protected override void OnExitState()
    {
        _tween?.Kill();
        _tween = CreateTween();

        _tween.TweenProperty(backgroundFogMaterial, "shader_parameter/blackout_mix", exitDensity, exitDuration);
        
        Camera.ClearGlobalTransformOverride();
        Camera.SetPlayerModelVisibility(true);
        
        tvController.ExitTvState();
        Player.visualHandler.Visible = true;
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