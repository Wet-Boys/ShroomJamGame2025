using Godot;
using System;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

public partial class SwatGameState : BaseTvGameState
{
    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
    }

    public override void OnEnterState()
    {
        IsActive = true;
        MouseReleaser.Instance.SetLockedMode(Input.MouseModeEnum.Visible);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Fly.flyCount == 0)
        {
            ExitTv();
            CanActivate = false;
        }
    }
}
