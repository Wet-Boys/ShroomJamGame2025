using Godot;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.obby;

[GlobalClass]
public partial class ObbyGameState : BaseTvGameState
{
    public override void _Ready()
    {
        CanActivate = true;
    }

    public override void OnEnterState()
    {
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Captured);
        IsActive = true;
    }
}