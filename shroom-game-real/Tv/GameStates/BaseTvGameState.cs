using Godot;

namespace ShroomGameReal.Tv.GameStates;

[GlobalClass]
public partial class BaseTvGameState : Node3D
{
    /// <summary>
    /// True when this state is active in the Tv
    /// </summary>
    public bool IsActive { get; protected set; }
    
    public bool CanActivate { get; protected set; }
    
    public float TimeScale => GlobalGameState.Instance.GameTimeScale;

    [Signal]
    public delegate void OnExitTvEventHandler();

    public virtual void ExitTv()
    {
        IsActive = false;
        EmitSignalOnExitTv();
    }

    public virtual void OnEnterState()
    {
        
    }
}