using Godot;
using System;
using ShroomGameReal;
using ShroomGameReal.Tv.GameStates;

public partial class AvoidBallsGameState : BaseTvGameState
{
    public override void _Ready()
    {
        base._Ready();
        CanActivate = true;
        infoText = "Avoid!";
    }
    public override void OnEnterState()
    {
        IsActive = true;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void Failure()
    {
        if (GameFlowHandler.isInDreamSequence)
        {
            GameFlowHandler.instance.FinishMinigame(this, true);
        }
    }
}
