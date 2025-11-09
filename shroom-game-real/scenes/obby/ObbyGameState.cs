using Godot;
using Godot.Collections;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

namespace ShroomGameReal.scenes.obby;

[GlobalClass]
public partial class ObbyGameState : BaseTvGameState
{
    [Export]
    private Array<PackedScene> _potentialLevels;
    public override void _Ready()
    {
        CanActivate = true;
        var newLevel = _potentialLevels.PickRandom().Instantiate<Node3D>();
        GetParent().AddChild(newLevel);
    }

    public override void OnEnterState()
    {
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Captured);
        IsActive = true;
    }
}