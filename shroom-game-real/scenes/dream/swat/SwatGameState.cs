using Godot;
using System;
using ShroomGameReal;
using ShroomGameReal.Tv.GameStates;
using ShroomGameReal.Utilities;

public partial class SwatGameState : BaseTvGameState
{
    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
        infoText = "Swat!";
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
            if (GameFlowHandler.isInDreamSequence)
            {
                GameFlowHandler.instance.FinishMinigame(this, false);
            }
            else
                ExitTv();
            CanActivate = false;
        }
    }
    public override void Failure()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, true);
        }
    }
}
